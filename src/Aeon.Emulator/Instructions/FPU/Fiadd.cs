using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.FPU;

internal static class Fiadd
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("DE/0 m16", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void AddInt16(VirtualMachine vm, short value)
    {
        vm.Processor.FPU.ST0_Ref += value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("DA/0 m32", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void AddInt32(VirtualMachine vm, int value)
    {
        vm.Processor.FPU.ST0_Ref += value;
    }
}
