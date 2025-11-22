namespace Aeon.Emulator.Instructions.Arithmetic;

internal static class Sub
{
    [Opcode("2C al,ib|80/5 rmb,ib|28/r rmb,rb|2A/r rb,rmb", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void ByteSubtract(Processor p, ref byte dest, byte src)
    {
        byte uResult = (byte)((uint)dest - (uint)src);
        p.Flags.Update_Sub_Byte(dest, src, uResult);
        dest = uResult;
    }

    [Opcode("2D ax,iw|81/5 rmw,iw|83/5 rmw,ibx|29/r rmw,rw|2B/r rw,rmw", AddressSize = 16 | 32)]
    public static void WordSubtract(Processor p, ref ushort dest, ushort src)
    {
        ushort uResult = (ushort)((uint)dest - (uint)src);
        p.Flags.Update_Sub_Word(dest, src, uResult);
        dest = uResult;
    }
    [Alternate("WordSubtract", AddressSize = 16 | 32)]
    public static void DWordSubtract(Processor p, ref uint dest, uint src)
    {
        uint uResult = dest - src;
        p.Flags.Update_Sub_DWord(dest, src, uResult);
        dest = uResult;
    }
}
