using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Aeon.Emulator.DebugSupport;

namespace Aeon.Emulator.Decoding
{
    /// <summary>
    /// Provides methods for decoding and emulating machine code.
    /// </summary>
    public static class InstructionSet
    {
        static InstructionSet()
        {
            var isb = new InstructionSetBuilder();
            isb.BuildSet();

            foreach(var info in isb.OneByteOpcodes)
                oneByteCodes[info.Opcode] = new OpcodeInfo(info);

            foreach(var info in isb.TwoByteOpcodes)
            {
                int byte1 = info.Opcode & 0xFF;
                int byte2 = (info.Opcode >> 8) & 0xFF;

                if(info.ModRmByte != ModRmInfo.OnlyRm)
                {
                    if(twoByteCodes[byte1] == null)
                        twoByteCodes[byte1] = new OpcodeInfo[256];
                    twoByteCodes[byte1][byte2] = new OpcodeInfo(info);
                }
                else
                {
                    if(twoByteRmCodes[byte1] == null)
                        twoByteRmCodes[byte1] = new OpcodeInfo[256][];
                    if(twoByteRmCodes[byte1][byte2] == null)
                        twoByteRmCodes[byte1][byte2] = new OpcodeInfo[8];
                    twoByteRmCodes[byte1][byte2][info.ExtendedRmOpcode] = new OpcodeInfo(info);
                }
            }

            foreach(var info in isb.ExtendedOpcodes)
            {
                if(rmCodes[info.Opcode] == null)
                    rmCodes[info.Opcode] = new OpcodeInfo[8];
                rmCodes[info.Opcode][info.ExtendedRmOpcode] = new OpcodeInfo(info);
            }

            allCodes = new OpcodeCollection();
        }

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

        internal static void Emulate(VirtualMachine vm)
        {
            var processor = vm.Processor;
            var memory = vm.PhysicalMemory;

            unsafe
            {
                uint* eip = (uint*)processor.PIP;
                uint startEIP = *eip;
                processor.StartEIP = startEIP;

                byte* ip = processor.CachedInstruction;
                memory.FetchInstruction(processor.segmentBases[(int)SegmentIndex.CS] + startEIP, ip);

                byte byte1 = ip[0];
                var inst = oneByteCodes[byte1];
                if(inst != null)
                {
                    *eip = startEIP + 1;
                    processor.CachedIP = ip + 1;
                    inst.Emulators[processor.SizeModeIndex](vm);
                    return;
                }

                var instSet = twoByteCodes[byte1];
                if(instSet != null)
                {
                    inst = instSet[ip[1]];
                    if(inst != null)
                    {
                        *eip = startEIP + 2;
                        processor.CachedIP = ip + 2;
                        inst.Emulators[processor.SizeModeIndex](vm);
                        return;
                    }
                }

                instSet = rmCodes[byte1];
                if(instSet != null)
                {
                    inst = instSet[(ip[1] & ModRmMask) >> 3];
                    if(inst == null)
                        throw GetOpcodeException(ip);

                    *eip = startEIP + 1;
                    processor.CachedIP = ip + 1;
                    inst.Emulators[processor.SizeModeIndex](vm);
                    return;
                }

                var instSetSet = twoByteRmCodes[byte1];
                if(instSetSet != null)
                {
                    instSet = instSetSet[ip[1]];
                    if(instSet != null)
                    {
                        inst = instSet[(ip[2] & ModRmMask) >> 3];
                        if(inst != null)
                        {
                            *eip = startEIP + 2;
                            processor.CachedIP = ip + 2;
                            inst.Emulators[processor.SizeModeIndex](vm);
                            return;
                        }
                    }
                }

                throw GetOpcodeException(ip);
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

        internal static void InitializeNativeArrays()
        {
            unsafe
            {
                // Allocate first level (processor mode)
                OneBytePtrs = (IntPtr**)functionPointerAllocator.Allocate(IntPtr.Size * 4, IntPtr.Size).ToPointer();
                RmPtrs = (IntPtr***)functionPointerAllocator.Allocate(IntPtr.Size * 4, IntPtr.Size).ToPointer();
                TwoBytePtrs = (IntPtr***)functionPointerAllocator.Allocate(IntPtr.Size * 4, IntPtr.Size).ToPointer();
                TwoByteRmPtrs = (IntPtr****)functionPointerAllocator.Allocate(IntPtr.Size * 4, IntPtr.Size).ToPointer();

                for(int mode = 0; mode < 4; mode++)
                {
                    OneBytePtrs[mode] = (IntPtr*)functionPointerAllocator.Allocate(IntPtr.Size * 256, IntPtr.Size).ToPointer();
                    RmPtrs[mode] = (IntPtr**)functionPointerAllocator.Allocate(IntPtr.Size * 256, IntPtr.Size).ToPointer();
                    for(int firstByte = 0; firstByte < 256; firstByte++)
                    {
                        OneBytePtrs[mode][firstByte] = GetFunctionPointer(oneByteCodes[firstByte], mode);

                        if(rmCodes[firstByte] != null)
                        {
                            RmPtrs[mode][firstByte] = (IntPtr*)functionPointerAllocator.Allocate(IntPtr.Size * 8, IntPtr.Size).ToPointer();
                            for(int rm = 0; rm < 8; rm++)
                                RmPtrs[mode][firstByte][rm] = GetFunctionPointer(rmCodes[firstByte][rm], mode);
                        }
                    }

                    TwoBytePtrs[mode] = (IntPtr**)functionPointerAllocator.Allocate(IntPtr.Size * 256, IntPtr.Size).ToPointer();
                    for(int firstByte = 0; firstByte < 256; firstByte++)
                    {
                        if(twoByteCodes[firstByte] != null)
                        {
                            TwoBytePtrs[mode][firstByte] = (IntPtr*)functionPointerAllocator.Allocate(IntPtr.Size * 256, IntPtr.Size).ToPointer();

                            for(int secondByte = 0; secondByte < 256; secondByte++)
                                TwoBytePtrs[mode][firstByte][secondByte] = GetFunctionPointer(twoByteCodes[firstByte][secondByte], mode);
                        }
                    }

                    TwoByteRmPtrs[mode] = (IntPtr***)functionPointerAllocator.Allocate(IntPtr.Size * 256, IntPtr.Size).ToPointer();
                    for(int firstByte = 0; firstByte < 256; firstByte++)
                    {
                        if(twoByteRmCodes[firstByte] != null)
                        {
                            TwoByteRmPtrs[mode][firstByte] = (IntPtr**)functionPointerAllocator.Allocate(IntPtr.Size * 256, IntPtr.Size).ToPointer();

                            for(int secondByte = 0; secondByte < 256; secondByte++)
                            {
                                if(twoByteRmCodes[firstByte][secondByte] != null)
                                {
                                    TwoByteRmPtrs[mode][firstByte][secondByte] = (IntPtr*)functionPointerAllocator.Allocate(IntPtr.Size * 8, IntPtr.Size).ToPointer();
                                    for(int rm = 0; rm < 8; rm++)
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
                if(inst != null)
                {
                    *eip = startEIP + 1;
                    processor.CachedIP = ip + 1;
                    return inst;
                }

                var instSet = twoByteCodes[byte1];
                if(instSet != null)
                {
                    inst = instSet[ip[1]];
                    if(inst != null)
                    {
                        *eip = startEIP + 2;
                        processor.CachedIP = ip + 2;
                        return inst;
                    }
                }

                instSet = rmCodes[byte1];
                if(instSet != null)
                {
                    inst = instSet[(ip[1] & ModRmMask) >> 3];
                    if(inst == null)
                        throw GetOpcodeException(ip);

                    *eip = startEIP + 1;
                    processor.CachedIP = ip + 1;
                    return inst;
                }

                var instSetSet = twoByteRmCodes[byte1];
                if(instSetSet != null)
                {
                    instSet = instSetSet[ip[1]];
                    if(instSet != null)
                    {
                        inst = instSet[(ip[2] & ModRmMask) >> 3];
                        if(inst != null)
                        {
                            *eip = startEIP + 2;
                            processor.CachedIP = ip + 2;
                            return inst;
                        }
                    }
                }

                throw GetOpcodeException(ip);
            }
        }
        private static IntPtr GetFunctionPointer(OpcodeInfo opcode, int mode)
        {
            if(opcode == null)
                return IntPtr.Zero;

            if(opcode.Emulators[mode] != null)
                return (IntPtr)typeof(Delegate).GetField("_methodPtrAux", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(opcode.Emulators[mode]);

            return IntPtr.Zero;
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static unsafe Exception GetOpcodeException(byte* ip) => new NotImplementedException($"Opcode {ip[0]:X2} {ip[1]:X2} ({ip[2]:X2}) not implemented.");

        private static readonly OpcodeInfo[] oneByteCodes = new OpcodeInfo[256];
        private static readonly OpcodeInfo[][] rmCodes = new OpcodeInfo[256][];
        private static readonly OpcodeInfo[][] twoByteCodes = new OpcodeInfo[256][];
        private static readonly OpcodeInfo[][][] twoByteRmCodes = new OpcodeInfo[256][][];
        private static readonly OpcodeCollection allCodes;

        private static readonly NativeHeap functionPointerAllocator = new NativeHeap(122880);
        internal unsafe static IntPtr** OneBytePtrs;
        internal unsafe static IntPtr*** RmPtrs;
        internal unsafe static IntPtr*** TwoBytePtrs;
        internal unsafe static IntPtr**** TwoByteRmPtrs;

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
                    if(this.count == 0)
                        this.count = this.Count();
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
