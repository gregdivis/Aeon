namespace Aeon.Emulator.Instructions.Arithmetic;

internal static class Sbb
{
    [Opcode("1C al,ib|80/3 rmb,ib|18/r rmb,rb|1A/r rb,rmb", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void ByteCarrySub(Processor p, ref byte dest, byte src)
    {
        uint c = p.Flags.Carry ? 1u : 0u;
        uint uResult = (uint)dest - (uint)src - c;
        p.Flags.Update_Sbb_Byte(dest, src, c, (byte)uResult);
        dest = (byte)(uResult & 0xFFu);
    }

    [Opcode("1D ax,iw|81/3 rmw,iw|83/3 rmw,ibx|19/r rmw,rw|1B/r rw,rmw", AddressSize = 16 | 32)]
    public static void WordCarrySub(Processor p, ref ushort dest, ushort src)
    {
        uint c = p.Flags.Carry ? 1u : 0u;
        uint uResult = (uint)dest - (uint)src - c;
        p.Flags.Update_Sbb_Word(dest, src, c, (ushort)uResult);
        dest = (ushort)(uResult & 0xFFFFu);
    }
    [Alternate(nameof(WordCarrySub), AddressSize = 16 | 32)]
    public static void DWordCarrySub(Processor p, ref uint dest, uint src)
    {
        uint c = p.Flags.Carry ? 1u : 0u;
        ulong uResult = (ulong)dest - (ulong)src - (ulong)c;
        p.Flags.Update_Sbb_DWord(dest, src, c, (uint)uResult);
        dest = (uint)(uResult & 0xFFFFFFFFu);
    }
}
