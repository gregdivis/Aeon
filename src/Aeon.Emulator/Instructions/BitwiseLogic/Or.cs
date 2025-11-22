namespace Aeon.Emulator.Instructions.BitwiseLogic;

internal static class Or
{
    [Opcode("08/r rmb,rb|0A/r rb,rmb|0C al,ib|80/1 rmb,ib", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void ByteOr(Processor p, ref byte dest, byte src)
    {
        dest |= src;
        p.Flags.Update_Value_Byte(dest);
        p.Flags.Carry = false;
        p.Flags.Overflow = false;
    }

    [Opcode("09/r rmw,rw|0B/r rw,rmw|0D ax,iw|81/1 rmw,iw|83/1 rmw,ibx", AddressSize = 16 | 32)]
    public static void WordOr(Processor p, ref ushort dest, ushort src)
    {
        dest |= src;
        p.Flags.Update_Value_Word(dest);
        p.Flags.Carry = false;
        p.Flags.Overflow = false;
    }
    [Alternate(nameof(WordOr), AddressSize = 16 | 32)]
    public static void DWordOr(Processor p, ref uint dest, uint src)
    {
        dest |= src;
        p.Flags.Update_Value_DWord(dest);
        p.Flags.Carry = false;
        p.Flags.Overflow = false;
    }
}
