using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.FPU;

internal static class Fimul
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("DE/1 m16", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void MultiplyInt16(Processor p, short value)
    {
        p.FPU.ST0_Ref *= value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("DA/1 m32", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void MultiplyInt32(Processor p, int value)
    {
        p.FPU.ST0_Ref *= value;
    }
}
