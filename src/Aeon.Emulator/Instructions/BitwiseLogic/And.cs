namespace Aeon.Emulator.Instructions.BitwiseLogic;

internal static class And
{
    [Opcode("20/r rmb,rb|22/r rb,rmb|24 al,ib|80/4 rmb,ib", AddressSize = 16 | 32, OperandSize = 16 | 32)]
    public static void ByteAnd(Processor p, ref byte dest, byte src)
    {
        dest &= src;
        p.Flags.Update_Value_Byte(dest);
        p.Flags.Carry = false;
        p.Flags.Overflow = false;
    }
    
    [Opcode("21/r rmw,rw|23/r rw,rmw|25 ax,iw|81/4 rmw,iw|83/4 rmw,ibx", AddressSize = 16 | 32)]
    public static void WordAnd(Processor p, ref ushort dest, ushort src)
    {
        dest &= src;
        p.Flags.Update_Value_Word(dest);
        p.Flags.Carry = false;
        p.Flags.Overflow = false;
    }

    [Alternate(nameof(WordAnd), AddressSize = 16 | 32)]
    public static void DWordAnd(Processor p, ref uint dest, uint src)
    {
        dest &= src;
        p.Flags.Update_Value_DWord(dest);
        p.Flags.Carry = false;
        p.Flags.Overflow = false;
    }
}
