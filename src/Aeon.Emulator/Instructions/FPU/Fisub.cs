namespace Aeon.Emulator.Instructions.FPU;

internal static class Fisub
{
    [Opcode("DE/4 m16", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void SubtractInt16(Processor p, short value)
    {
        p.FPU.ST0_Ref -= value;
    }

    [Opcode("DA/4 m32", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void SubtractInt32(Processor p, int value)
    {
        p.FPU.ST0_Ref -= value;
    }
}
