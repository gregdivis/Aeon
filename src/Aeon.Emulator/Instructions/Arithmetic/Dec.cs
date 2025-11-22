namespace Aeon.Emulator.Instructions.Arithmetic;

internal static class Dec
{
    [Opcode("FE/1 rmb", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void ByteDecrement(Processor p, ref byte dest)
    {
        uint uResult = (uint)dest - 1u;
        p.Flags.Update_Dec_Byte(dest, (byte)uResult);
        dest = (byte)uResult;
    }

    [Opcode("48+ rw|FF/1 rmw", AddressSize = 16 | 32)]
    public static void WordDecrement(Processor p, ref ushort dest)
    {
        uint uResult = (uint)dest - 1u;
        p.Flags.Update_Dec_Word(dest, (ushort)uResult);
        dest = (ushort)uResult;
    }
    [Alternate(nameof(WordDecrement), AddressSize = 16 | 32)]
    public static void DWordDecrement(Processor p, ref uint dest)
    {
        uint uResult = dest - 1u;
        p.Flags.Update_Dec_DWord(dest, uResult);
        dest = uResult;
    }
}
