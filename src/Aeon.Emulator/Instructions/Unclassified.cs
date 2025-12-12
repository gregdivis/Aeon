using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using Aeon.Emulator.RuntimeExceptions;

namespace Aeon.Emulator.Instructions;

internal static class UnclassifiedInstructions
{
    [Opcode("F4", Name = "hlt", AddressSize = 16 | 32, OperandSize = 16 | 32)]
    public static void Halt(VirtualMachine vm)
    {
        if (vm.Processor.CR0.HasFlag(CR0.ProtectedModeEnable))
        {
            uint cpl = vm.Processor.CS & 0x3u;
            if (cpl != 0)
            {
                vm.RaiseException(new GeneralProtectionFaultException(0));
                return;
            }
        }

        ThrowHelper.ThrowHaltException();
    }

    [Opcode("9B", Name = "wait", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void Wait(VirtualMachine vm)
    {
    }

    [Opcode("D7", Name = "xlat", OperandSize = 16 | 32, AddressSize = 16)]
    public static void TranslateByte(VirtualMachine vm)
    {
        ushort offset = (ushort)vm.Processor.BX;
        offset += vm.Processor.AL;
        var baseAddress = vm.Processor.GetOverrideBase(SegmentIndex.DS);
        vm.Processor.AL = vm.PhysicalMemory.GetByte(baseAddress + offset);

        vm.Processor.InstructionEpilog();
    }
    [Alternate(nameof(TranslateByte), OperandSize = 16 | 32, AddressSize = 32)]
    public static void TranslateByte32(VirtualMachine vm)
    {
        uint offset = (uint)vm.Processor.EBX;
        offset += vm.Processor.AL;
        var baseAddress = vm.Processor.GetOverrideBase(SegmentIndex.DS);
        vm.Processor.AL = vm.PhysicalMemory.GetByte(baseAddress + offset);

        vm.Processor.InstructionEpilog();
    }

    [Opcode("C9", Name = "leave", AddressSize = 16 | 32)]
    public static void Leave(VirtualMachine vm)
    {
        if (vm.BigStackPointer)
            vm.Processor.ESP = vm.Processor.EBP;
        else
            vm.Processor.SP = vm.Processor.BP;

        vm.Processor.BP = vm.PopFromStack();

        vm.Processor.InstructionEpilog();
    }
    [Alternate(nameof(Leave), AddressSize = 16 | 32)]
    public static void Leave32(VirtualMachine vm)
    {
        if (vm.BigStackPointer)
            vm.Processor.ESP = vm.Processor.EBP;
        else
            vm.Processor.SP = vm.Processor.BP;

        vm.Processor.EBP = vm.PopFromStack32();

        vm.Processor.InstructionEpilog();
    }

    [Opcode("0FA2", Name = "cpuid", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void CPUID(VirtualMachine vm)
    {
        switch (vm.Processor.EAX)
        {
            case 0:
                vm.Processor.EAX = 1;
                vm.Processor.EBX = 0x756E6547;
                vm.Processor.EDX = 0x49656E69;
                vm.Processor.ECX = 0x6C65746E;
                break;

            case 1:
                vm.Processor.EAX = 0x00000400; // This should be a 486DX
                vm.Processor.EDX = 0x00000001; // FPU is present
                break;
        }

        vm.Processor.InstructionEpilog();
    }

    [Opcode("0FC8+ rw", Name = "bswap", OperandSize = 16, AddressSize = 16 | 32)]
    public static void ByteSwap(VirtualMachine vm, ref ushort value)
    {
        throw new NotSupportedException();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Alternate(nameof(ByteSwap), OperandSize = 32, AddressSize = 16 | 32)]
    public static void ByteSwap32(VirtualMachine vm, ref uint value)
    {
        value = BinaryPrimitives.ReverseEndianness(value);
    }
}
