using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Aeon.Emulator.DebugSupport;

#nullable disable

namespace Aeon.Emulator.Decoding
{
    /// <summary>
    /// Provides methods for decoding and emulating machine code.
    /// </summary>
    public static partial class InstructionSet
    {
        private static bool initialized;

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
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.NoInlining)]
        internal static void Emulate(VirtualMachine vm, uint count)
        {
            var processor = vm.Processor;
            var memory = vm.PhysicalMemory;

            unsafe
            {
                ref uint eip = ref processor.EIP;

                for (uint i = 0; i < count; i++)
                {
                    uint startEIP = eip;
                    processor.StartEIP = startEIP;

                    byte* ip = processor.CachedInstruction;
                    memory.FetchInstruction(processor.CSBase + startEIP, ip);

                    uint sizeModeIndex = processor.SizeModeIndex;

                    byte byte1 = ip[0];
                    var inst = OneBytePtrs[sizeModeIndex][byte1];
                    if (inst != null)
                    {
                        eip = startEIP + 1;
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
                            eip = startEIP + 2;
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
                        {
                            ThrowGetOpcodeException(ip);
                            return;
                        }

                        eip = startEIP + 1;
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
                                eip = startEIP + 2;
                                processor.CachedIP = ip + 2;
                                inst(vm);
                                continue;
                            }
                        }
                    }

                    ThrowGetOpcodeException(ip);
                    return;
                }
            }
        }

        internal static void Emulate(VirtualMachine vm, InstructionLog log) => throw new NotImplementedException();

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
                OneBytePtrs = (delegate*<VirtualMachine, void>**)alloc(4);
                RmPtrs = (delegate*<VirtualMachine, void>***)alloc(4);
                TwoBytePtrs = (delegate*<VirtualMachine, void>***)alloc(4);
                TwoByteRmPtrs = (delegate*<VirtualMachine, void>****)alloc(4);

                for (int mode = 0; mode < 4; mode++)
                {
                    OneBytePtrs[mode] = (delegate*<VirtualMachine, void>*)alloc(256);
                    RmPtrs[mode] = (delegate*<VirtualMachine, void>**)alloc(256);
                    TwoBytePtrs[mode] = (delegate*<VirtualMachine, void>**)alloc(256);
                    TwoByteRmPtrs[mode] = (delegate*<VirtualMachine, void>***)alloc(256);
                }

                GetOneBytePointers(OneBytePtrs);
                GetOneByteRmPointers(RmPtrs, alloc);
                GetTwoBytePointers(TwoBytePtrs, alloc);
                GetTwoByteRmPointers(TwoByteRmPtrs, alloc);

                static nint alloc(int count) => functionPointerAllocator.Allocate(sizeof(nint) * count, sizeof(nint));
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

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static unsafe void ThrowGetOpcodeException(byte* ip) => throw new NotImplementedException($"Opcode {ip[0]:X2} {ip[1]:X2} ({ip[2]:X2}) not implemented.");

        private static unsafe partial void GetOneBytePointers(delegate*<VirtualMachine, void>** ptrs);
        private static unsafe partial void GetOneByteRmPointers(delegate*<VirtualMachine, void>*** ptrs, Func<int, nint> alloc);
        private static unsafe partial void GetTwoBytePointers(delegate*<VirtualMachine, void>*** ptrs, Func<int, nint> alloc);
        private static unsafe partial void GetTwoByteRmPointers(delegate*<VirtualMachine, void>**** ptrs, Func<int, nint> alloc);


#pragma warning disable IDE0044 // Add readonly modifier
        // these are not readonly because accessing them is performance critical
        private static OpcodeInfo[] oneByteCodes = new OpcodeInfo[256];
        private static OpcodeInfo[][] rmCodes = new OpcodeInfo[256][];
        private static OpcodeInfo[][] twoByteCodes = new OpcodeInfo[256][];
        private static OpcodeInfo[][][] twoByteRmCodes = new OpcodeInfo[256][][];
        private static OpcodeCollection allCodes;
#pragma warning restore IDE0044 // Add readonly modifier

        private static readonly NativeHeap functionPointerAllocator = new(122880);
        private unsafe static delegate*<VirtualMachine, void>** OneBytePtrs;
        private unsafe static delegate*<VirtualMachine, void>*** RmPtrs;
        private unsafe static delegate*<VirtualMachine, void>*** TwoBytePtrs;
        private unsafe static delegate*<VirtualMachine, void>**** TwoByteRmPtrs;

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
