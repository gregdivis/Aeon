namespace Aeon.Emulator.Instructions.Arithmetic;

internal static class Add
{
    [Opcode("00/r rmb,rb|02/r rb,rmb|04 al,ib|80/0 rmb,ib", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void ByteAdd(Processor p, ref byte dest, byte src)
    {
        uint uResult = (uint)dest + (uint)src;
        p.Flags.Update_Add_Byte(dest, src, (byte)uResult);
        dest = (byte)uResult;
    }
    
    [Opcode("01/r rmw,rw|03/r rw,rmw|05 ax,iw|81/0 rmw,iw|83/0 rmw,ibx", AddressSize = 16 | 32)]
    public static void WordAdd(Processor p, ref ushort dest, ushort src)
    {
        uint uResult = (uint)dest + (uint)src;
        p.Flags.Update_Add_Word(dest, src, (ushort)uResult);
        dest = (ushort)uResult;
    }
    [Alternate(nameof(WordAdd), AddressSize = 16 | 32)]
    public static void DWordAdd(Processor p, ref uint dest, uint src)
    {
        uint uResult = dest + src;
        p.Flags.Update_Add_DWord(dest, src, uResult);
        dest = uResult;
    }
}
