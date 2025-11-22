namespace Aeon.Emulator.Instructions.Arithmetic;

internal static class Neg
{
    [Opcode("F6/3 rmb", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void ByteNegate(Processor p, ref sbyte dest)
    {
        sbyte sResult = (sbyte)-dest;
        p.Flags.Update_Sub_Byte(0, (byte)dest, (byte)sResult);
        dest = sResult;
    }

    [Opcode("F7/3 rmw", AddressSize = 16 | 32)]
    public static void WordNegate(Processor p, ref short dest)
    {
        short sResult = (short)-dest;
        p.Flags.Update_Sub_Word(0, (ushort)dest, (ushort)sResult);
        dest = sResult;
    }
    [Alternate(nameof(WordNegate), AddressSize = 16 | 32)]
    public static void DWordNegate(Processor p, ref int dest)
    {
        int sResult = -dest;
        p.Flags.Update_Sub_DWord(0, (uint)dest, (uint)sResult);
        dest = sResult;
    }
}
