namespace Aeon.Emulator.Instructions.Arithmetic;

internal static class Adc
{
    [Opcode("10/r rmb,rb|12/r rb,rmb|14 al,ib|80/2 rmb,ib", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void ByteCarryAdd(Processor p, ref byte dest, byte src)
    {
        uint c = p.Flags.Carry ? 1u : 0u;
        uint uResult = (uint)dest + (uint)src + c;
        p.Flags.Update_Adc_Byte(dest, src, c, (byte)uResult);
        dest = (byte)(uResult & 0xFFu);
    }

    [Opcode("11/r rmw,rw|13/r rw,rmw|15 ax,iw|81/2 rmw,iw|83/2 rmw,ibx", AddressSize = 16 | 32)]
    public static void WordCarryAdd(Processor p, ref ushort dest, ushort src)
    {
        uint c = p.Flags.Carry ? 1u : 0u;
        uint uResult = (uint)dest + (uint)src + c;
        p.Flags.Update_Adc_Word(dest, src, c, (ushort)uResult);
        dest = (ushort)(uResult & 0xFFFFu);
    }
    [Alternate(nameof(WordCarryAdd), AddressSize = 16 | 32)]
    public static void DWordCarryAdd(Processor p, ref uint dest, uint src)
    {
        uint c = p.Flags.Carry ? 1u : 0u;
        ulong uResult = (ulong)dest + (ulong)src + (ulong)c;
        p.Flags.Update_Adc_DWord(dest, src, c, (uint)uResult);
        dest = (uint)(uResult & 0xFFFFFFFFu);
    }
}
