using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Decoding;

/// <summary>
/// Provides methods for decoding and emulating machine code.
/// </summary>
public static partial class InstructionSet
{
    private static bool initialized;

    /// <summary>
    /// Core CPU emulation loop.
    /// </summary>
    /// <param name="vm"><see cref="VirtualMachine"/> instance.</param>
    /// <param name="count">Number of instructions to emulate.</param>
    [SkipLocalsInit]
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

    public static void Initialize()
    {
        if (initialized)
            return;

        initialized = true;
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

    [DoesNotReturn]
    internal static unsafe void ThrowGetOpcodeException(byte* ip) => throw new NotImplementedException($"Opcode {ip[0]:X2} {ip[1]:X2} ({ip[2]:X2}) not implemented.");

    private static unsafe partial void GetOneBytePointers(delegate*<VirtualMachine, void>** ptrs);
    private static unsafe partial void GetOneByteRmPointers(delegate*<VirtualMachine, void>*** ptrs, Func<int, nint> alloc);
    private static unsafe partial void GetTwoBytePointers(delegate*<VirtualMachine, void>*** ptrs, Func<int, nint> alloc);
    private static unsafe partial void GetTwoByteRmPointers(delegate*<VirtualMachine, void>**** ptrs, Func<int, nint> alloc);

    private static readonly NativeHeap functionPointerAllocator = new(122880);
    private unsafe static delegate*<VirtualMachine, void>** OneBytePtrs;
    private unsafe static delegate*<VirtualMachine, void>*** RmPtrs;
    private unsafe static delegate*<VirtualMachine, void>*** TwoBytePtrs;
    private unsafe static delegate*<VirtualMachine, void>**** TwoByteRmPtrs;
}
