using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.FPU;

internal static class Fidiv
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("DE/6 m16", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void DivideInt16(Processor p, short value)
    {
        p.FPU.ST0_Ref /= value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("DA/6 m32", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void DivideInt32(Processor p, int value)
    {
        p.FPU.ST0_Ref /= value;
    }
}
