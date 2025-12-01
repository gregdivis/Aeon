namespace Aeon.Emulator.Instructions;

internal static class Cmp
{
    [Opcode("38/r rmb,rb|3A/r rb,rmb|3C al,ib|80/7 rmb,ib|82/7 rmb,ib", AddressSize = 16 | 32, OperandSize = 16 | 32)]
    public static void ByteCompare(Processor p, byte value1, byte value2)
    {
        byte uResult = (byte)((uint)value1 - (uint)value2);
        p.Flags.Update_Sub_Byte(value1, value2, uResult);
    }

    [Opcode("39/r rmw,rw|3B/r rw,rmw|3D ax,iw|81/7 rmw,iw|83/7 rmw,ibx", AddressSize = 16 | 32)]
    public static void WordCompare(Processor p, ushort value1, ushort value2)
    {
        ushort uResult = (ushort)((uint)value1 - (uint)value2);
        p.Flags.Update_Sub_Word(value1, value2, uResult);
    }
    [Alternate(nameof(WordCompare), AddressSize = 16 | 32)]
    public static void DWordCompare(Processor p, uint value1, uint value2)
    {
        uint uResult = value1 - value2;
        p.Flags.Update_Sub_DWord(value1, value2, uResult);
    }
}

internal static class Test
{
    [Opcode("A8 al,ib|F6/0 rmb,ib|84/r rmb,rb", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void ByteTest(Processor p, byte value1, byte value2)
    {
        byte result = (byte)(value1 & value2);
        p.Flags.Update_Value_Byte(result);
        p.Flags.Carry = false;
        p.Flags.Overflow = false;
    }

    [Opcode("A9 ax,iw|F7/0 rmw,iw|85/r rmw,rw", AddressSize = 16 | 32)]
    public static void WordTest(Processor p, ushort value1, ushort value2)
    {
        ushort result = (ushort)(value1 & value2);
        p.Flags.Update_Value_Word(result);
        p.Flags.Carry = false;
        p.Flags.Overflow = false;
    }
    [Alternate(nameof(WordTest), AddressSize = 16 | 32)]
    public static void DWordTest(Processor p, uint value1, uint value2)
    {
        uint result = value1 & value2;
        p.Flags.Update_Value_DWord(result);
        p.Flags.Carry = false;
        p.Flags.Overflow = false;
    }
}
