namespace Aeon.Emulator.Instructions.Arithmetic;

internal static class Inc
{
    [Opcode("FE/0 rmb", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void ByteIncrement(Processor p, ref byte dest)
    {
        uint uResult = (uint)dest + (uint)1;
        p.Flags.Update_Inc_Byte(dest, (byte)uResult);
        dest = (byte)(uResult & 0xFF);
    }

    [Opcode("40+ rw|FF/0 rmw", AddressSize = 16 | 32)]
    public static void WordIncrement(Processor p, ref ushort dest)
    {
        uint uResult = (uint)dest + (uint)1;
        p.Flags.Update_Inc_Word(dest, (ushort)uResult);
        dest = (ushort)(uResult & 0xFFFF);
    }
    [Alternate(nameof(WordIncrement), AddressSize = 16 | 32)]
    public static void DWordIncrement(Processor p, ref uint dest)
    {
        uint uResult = dest + 1U;
        p.Flags.Update_Inc_DWord(dest, uResult);
        dest = uResult;
    }
}
