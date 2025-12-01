namespace Aeon.Emulator.Instructions.FPU;

internal static class Fisubr
{
    [Opcode("DE/5 m16", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void ReverseSubtractInt16(Processor p, short value)
    {
        ref var st0 = ref p.FPU.ST0_Ref;
        st0 = value - st0;
    }

    [Opcode("DA/5 m32", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void ReverseSubtractInt32(Processor p, int value)
    {
        ref var st0 = ref p.FPU.ST0_Ref;
        st0 = value - st0;
    }
}
