using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Aeon.Emulator.DebugSupport;

#nullable disable

namespace Aeon.Emulator.Decoding
{
    /// <summary>
    /// Provides methods for decoding and emulating machine code.
    /// </summary>
    public static class InstructionSet
    {
        private static bool initialized;

        /// <summary>
        /// Gets the collection of defined opcodes.
        /// </summary>
        public static OpcodeCollection Opcodes => allCodes;

        public static OpcodeInfo Decode(ReadOnlySpan<byte> machineCode)
        {
            byte byte1 = machineCode[0];
            var inst = oneByteCodes[byte1];
            if (inst != null)
                return inst;

            var instSet = twoByteCodes[byte1];
            if (instSet != null)
            {
                inst = instSet[machineCode[1]];
                if (inst != null)
                    return inst;
            }

            instSet = rmCodes[byte1];
            if (instSet != null)
            {
                inst = instSet[(machineCode[1] & ModRmMask) >> 3];
                if (inst == null)
                    return null;

                return inst;
            }

            var instSetSet = twoByteRmCodes[byte1];
            if (instSetSet != null)
            {
                instSet = instSetSet[machineCode[1]];
                if (instSet != null)
                {
                    inst = instSet[(machineCode[2] & ModRmMask) >> 3];
                    if (inst != null)
                        return inst;
                }
            }

            return null;
        }

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        internal static void Emulate(VirtualMachine vm, uint count)
        {
            var processor = vm.Processor;
            var memory = vm.PhysicalMemory;

            unsafe
            {
                uint* eip = (uint*)processor.PIP;

                for (uint i = 0; i < count; i++)
                {
                    uint startEIP = *eip;
                    processor.StartEIP = startEIP;

                    byte* ip = processor.CachedInstruction;
                    memory.FetchInstruction(processor.segmentBases[(int)SegmentIndex.CS] + startEIP, ip);

                    uint sizeModeIndex = processor.SizeModeIndex;

                    byte byte1 = ip[0];
                    var inst = OneBytePtrs[sizeModeIndex][byte1];
                    if (inst != null)
                    {
                        *eip = startEIP + 1;
                        processor.CachedIP = ip + 1;
                        inst(vm);
                        continue;
                    }

                    var instSet = TwoBytePtrs[sizeModeIndex][byte1];
                    if (instSet != null)
                    {
                        inst = instSet[ip[1]];
                        if (inst != null)
                        {
                            *eip = startEIP + 2;
                            processor.CachedIP = ip + 2;
                            inst(vm);
                            continue;
                        }
                    }

                    instSet = RmPtrs[sizeModeIndex][byte1];
                    if (instSet != null)
                    {
                        inst = instSet[Intrinsics.ExtractBits(ip[1], 3, 3, 0x38)];
                        if (inst == null)
                            ThrowGetOpcodeException(ip);

                        *eip = startEIP + 1;
                        processor.CachedIP = ip + 1;
                        inst(vm);
                        continue;
                    }

                    var instSetSet = TwoByteRmPtrs[sizeModeIndex][byte1];
                    if (instSetSet != null)
                    {
                        instSet = instSetSet[ip[1]];
                        if (instSet != null)
                        {
                            inst = instSet[Intrinsics.ExtractBits(ip[2], 3, 3, 0x38)];
                            if (inst != null)
                            {
                                *eip = startEIP + 2;
                                processor.CachedIP = ip + 2;
                                inst(vm);
                                continue;
                            }
                        }
                    }

                    ThrowGetOpcodeException(ip);
                }
            }
        }

        internal static void Emulate(VirtualMachine vm, InstructionLog log)
        {
            var info = FindOpcode(vm.PhysicalMemory, vm.Processor);
            if (!info.IsPrefix)
                log.Write(vm.Processor);

            var method = info.Emulators[vm.Processor.SizeModeIndex];
            if (method != null)
                method(vm);
            else
                throw GetPartiallyNotImplementedException(vm, info);
        }

        public static void Initialize()
        {
            if (initialized)
                return;

            initialized = true;

            var isb = new InstructionSetBuilder();
            isb.BuildSet();

            foreach (var info in isb.OneByteOpcodes)
                oneByteCodes[info.Opcode] = new OpcodeInfo(info);

            foreach (var info in isb.TwoByteOpcodes)
            {
                int byte1 = info.Opcode & 0xFF;
                int byte2 = (info.Opcode >> 8) & 0xFF;

                if (info.ModRmByte != ModRmInfo.OnlyRm)
                {
                    if (twoByteCodes[byte1] == null)
                        twoByteCodes[byte1] = new OpcodeInfo[256];
                    twoByteCodes[byte1][byte2] = new OpcodeInfo(info);
                }
                else
                {
                    if (twoByteRmCodes[byte1] == null)
                        twoByteRmCodes[byte1] = new OpcodeInfo[256][];
                    if (twoByteRmCodes[byte1][byte2] == null)
                        twoByteRmCodes[byte1][byte2] = new OpcodeInfo[8];
                    twoByteRmCodes[byte1][byte2][info.ExtendedRmOpcode] = new OpcodeInfo(info);
                }
            }

            foreach (var info in isb.ExtendedOpcodes)
            {
                if (rmCodes[info.Opcode] == null)
                    rmCodes[info.Opcode] = new OpcodeInfo[8];
                rmCodes[info.Opcode][info.ExtendedRmOpcode] = new OpcodeInfo(info);
            }

            allCodes = new OpcodeCollection();

            InitializeNativeArrays();
            RegRmw16Loads.Initialize();
            RegRmw32Loads.Initialize();
        }

        private static void InitializeNativeArrays()
        {
            unsafe
            {
                // Allocate first level (processor mode)
                OneBytePtrs = (delegate*<VirtualMachine, void>**)functionPointerAllocator.Allocate(IntPtr.Size * 4, IntPtr.Size).ToPointer();
                RmPtrs = (delegate*<VirtualMachine, void>***)functionPointerAllocator.Allocate(IntPtr.Size * 4, IntPtr.Size).ToPointer();
                TwoBytePtrs = (delegate*<VirtualMachine, void>***)functionPointerAllocator.Allocate(IntPtr.Size * 4, IntPtr.Size).ToPointer();
                TwoByteRmPtrs = (delegate*<VirtualMachine, void>****)functionPointerAllocator.Allocate(IntPtr.Size * 4, IntPtr.Size).ToPointer();

                for (int mode = 0; mode < 4; mode++)
                {
                    OneBytePtrs[mode] = (delegate*<VirtualMachine, void>*)functionPointerAllocator.Allocate(IntPtr.Size * 256, IntPtr.Size).ToPointer();
                    RmPtrs[mode] = (delegate*<VirtualMachine, void>**)functionPointerAllocator.Allocate(IntPtr.Size * 256, IntPtr.Size).ToPointer();
                    for (int firstByte = 0; firstByte < 256; firstByte++)
                    {
                        OneBytePtrs[mode][firstByte] = GetFunctionPointer(oneByteCodes[firstByte], mode);

                        if (rmCodes[firstByte] != null)
                        {
                            RmPtrs[mode][firstByte] = (delegate*<VirtualMachine, void>*)functionPointerAllocator.Allocate(IntPtr.Size * 8, IntPtr.Size).ToPointer();
                            for (int rm = 0; rm < 8; rm++)
                                RmPtrs[mode][firstByte][rm] = GetFunctionPointer(rmCodes[firstByte][rm], mode);
                        }
                    }

                    TwoBytePtrs[mode] = (delegate*<VirtualMachine, void>**)functionPointerAllocator.Allocate(IntPtr.Size * 256, IntPtr.Size).ToPointer();
                    for (int firstByte = 0; firstByte < 256; firstByte++)
                    {
                        if (twoByteCodes[firstByte] != null)
                        {
                            TwoBytePtrs[mode][firstByte] = (delegate*<VirtualMachine, void>*)functionPointerAllocator.Allocate(IntPtr.Size * 256, IntPtr.Size).ToPointer();

                            for (int secondByte = 0; secondByte < 256; secondByte++)
                                TwoBytePtrs[mode][firstByte][secondByte] = GetFunctionPointer(twoByteCodes[firstByte][secondByte], mode);
                        }
                    }

                    TwoByteRmPtrs[mode] = (delegate*<VirtualMachine, void>***)functionPointerAllocator.Allocate(IntPtr.Size * 256, IntPtr.Size).ToPointer();
                    for (int firstByte = 0; firstByte < 256; firstByte++)
                    {
                        if (twoByteRmCodes[firstByte] != null)
                        {
                            TwoByteRmPtrs[mode][firstByte] = (delegate*<VirtualMachine, void>**)functionPointerAllocator.Allocate(IntPtr.Size * 256, IntPtr.Size).ToPointer();

                            for (int secondByte = 0; secondByte < 256; secondByte++)
                            {
                                if (twoByteRmCodes[firstByte][secondByte] != null)
                                {
                                    TwoByteRmPtrs[mode][firstByte][secondByte] = (delegate*<VirtualMachine, void>*)functionPointerAllocator.Allocate(IntPtr.Size * 8, IntPtr.Size).ToPointer();
                                    for (int rm = 0; rm < 8; rm++)
                                        TwoByteRmPtrs[mode][firstByte][secondByte][rm] = GetFunctionPointer(twoByteRmCodes[firstByte][secondByte][rm], mode);
                                }
                            }
                        }
                    }
                }
            }
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception GetPartiallyNotImplementedException(VirtualMachine vm, OpcodeInfo info) => new NotImplementedException($"Instruction '{info.Name}' not implemented for {vm.Processor.AddressSize}-bit addressing, {vm.Processor.OperandSize}-bit operand size.");
        private static OpcodeInfo FindOpcode(PhysicalMemory memory, Processor processor)
        {
            unsafe
            {
                uint* eip = (uint*)processor.PIP;
                uint startEIP = *eip;
                processor.StartEIP = startEIP;

                byte* ip = processor.CachedInstruction;
                memory.FetchInstruction(processor.segmentBases[(int)SegmentIndex.CS] + startEIP, ip);

                byte byte1 = ip[0];
                var inst = oneByteCodes[byte1];
                if (inst != null)
                {
                    *eip = startEIP + 1;
                    processor.CachedIP = ip + 1;
                    return inst;
                }

                var instSet = twoByteCodes[byte1];
                if (instSet != null)
                {
                    inst = instSet[ip[1]];
                    if (inst != null)
                    {
                        *eip = startEIP + 2;
                        processor.CachedIP = ip + 2;
                        return inst;
                    }
                }

                instSet = rmCodes[byte1];
                if (instSet != null)
                {
                    inst = instSet[(ip[1] & ModRmMask) >> 3];
                    if (inst == null)
                        ThrowGetOpcodeException(ip);

                    *eip = startEIP + 1;
                    processor.CachedIP = ip + 1;
                    return inst;
                }

                var instSetSet = twoByteRmCodes[byte1];
                if (instSetSet != null)
                {
                    instSet = instSetSet[ip[1]];
                    if (instSet != null)
                    {
                        inst = instSet[(ip[2] & ModRmMask) >> 3];
                        if (inst != null)
                        {
                            *eip = startEIP + 2;
                            processor.CachedIP = ip + 2;
                            return inst;
                        }
                    }
                }

                ThrowGetOpcodeException(ip);
                return null;
            }
        }
        static readonly System.Reflection.FieldInfo methodPtr =
            // .NET
            typeof(Delegate).GetField("_methodPtrAux", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic) ??
            // Mono
            typeof(Delegate).GetField("interp_method", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        private static unsafe delegate*<VirtualMachine, void> GetFunctionPointer(OpcodeInfo opcode, int mode)
        {
            if (opcode == null)
                return null;

            if (opcode.Emulators[mode] != null)
                return (delegate*<VirtualMachine, void>)((IntPtr)methodPtr.GetValue(opcode.Emulators[mode])).ToPointer();

            return null;
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static unsafe void ThrowGetOpcodeException(byte* ip) => throw new NotImplementedException($"Opcode {ip[0]:X2} {ip[1]:X2} ({ip[2]:X2}) not implemented.");

#pragma warning disable IDE0044 // Add readonly modifier
        // these are not readonly because accessing them is performance critical
        private static OpcodeInfo[] oneByteCodes = new OpcodeInfo[256];
        private static OpcodeInfo[][] rmCodes = new OpcodeInfo[256][];
        private static OpcodeInfo[][] twoByteCodes = new OpcodeInfo[256][];
        private static OpcodeInfo[][][] twoByteRmCodes = new OpcodeInfo[256][][];
        private static OpcodeCollection allCodes;
#pragma warning restore IDE0044 // Add readonly modifier

        private static readonly NativeHeap functionPointerAllocator = new(122880);
        internal unsafe static delegate*<VirtualMachine, void>** OneBytePtrs;
        internal unsafe static delegate*<VirtualMachine, void>*** RmPtrs;
        internal unsafe static delegate*<VirtualMachine, void>*** TwoBytePtrs;
        internal unsafe static delegate*<VirtualMachine, void>**** TwoByteRmPtrs;

        private const int ModRmMask = 0x38;

        /// <summary>
        /// Contains all defined instruction opcodes.
        /// </summary>
        public sealed class OpcodeCollection : IEnumerable<OpcodeInfo>
        {
            private int count;
            private readonly IEnumerable<OpcodeInfo> allCodes;

            internal OpcodeCollection()
            {
                var codes = oneByteCodes.Where(IsNotNull);
                codes = Append(codes, Expand(twoByteCodes.Where(IsNotNull)).Where(IsNotNull));
                codes = Append(codes, Expand(rmCodes.Where(IsNotNull)).Where(IsNotNull));
                codes = Append(codes, Expand(Expand(twoByteRmCodes.Where(IsNotNull)).Where(IsNotNull)).Where(IsNotNull));

                this.allCodes = codes;
            }

            /// <summary>
            /// Gets information about an opcode.
            /// </summary>
            /// <param name="opcode">Opcode to retrieve.</param>
            /// <returns>Information about the supplied opcode if defined; otherwise null.</returns>
            public OpcodeInfo this[int opcode] => Decode(BitConverter.GetBytes(opcode));

            /// <summary>
            /// Gets the number of opcodes defined.
            /// </summary>
            public int Count
            {
                get
                {
                    if (this.count == 0)
                    {
                        int n = 0;
                        foreach (var _ in this)
                            n++;

                        this.count = n;
                    }

                    return this.count;
                }
            }

            /// <summary>
            /// Returns an enumerator for all defined opcodes.
            /// </summary>
            /// <returns>Enumerator for all defined opcodes.</returns>
            public IEnumerator<OpcodeInfo> GetEnumerator() => this.allCodes.GetEnumerator();
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

            private static bool IsNotNull(object obj) => obj != null;
            private static IEnumerable<T> Append<T>(IEnumerable<T> source1, IEnumerable<T> source2) => source1.Concat(source2);
            private static IEnumerable<T> Expand<T>(IEnumerable<IEnumerable<T>> source) => source.SelectMany(s => s);
        }
    }
}
