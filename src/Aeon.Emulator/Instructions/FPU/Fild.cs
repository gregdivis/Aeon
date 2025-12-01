namespace Aeon.Emulator.Instructions.FPU;

internal static class Fild
{
    [Opcode("DF/0 m16", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void LoadInt16(Processor p, short value)
    {
        p.FPU.Push(value);
    }

    [Opcode("DB/0 m32", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void LoadInt32(Processor p, int value)
    {
        p.FPU.Push(value);
    }

    [Opcode("DF/5 m64", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void LoadInt64(Processor p, long value)
    {
        p.FPU.Push(value);
    }
}
