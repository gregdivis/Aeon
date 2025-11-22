namespace Aeon.Emulator.Instructions.BitShifting;

internal static class Rcl
{
    [Opcode("D0/2 rmb", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void ByteRotateCarryLeft1(Processor p, ref byte dest)
    {
        uint buffer = dest;

        if (p.Flags.Carry)
            buffer |= 0x0100;

        uint b = buffer << 1;
        uint c = buffer >>> 8;
        buffer = b | c;

        dest = (byte)buffer;
        p.Flags.Carry = (buffer & 0x0100) == 0x0100;
        p.Flags.Overflow = (buffer & 0x0180) == 0x0100 || (buffer & 0x0180) == 0x0080;
    }
    [Opcode("D2/2 rmb,cl|C0/2 rmb,ib", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void ByteRotateCarryLeft(Processor p, ref byte dest, byte count)
    {
        count = (byte)((count & 0x1F) % 9);
        if (count == 0)
        {
            return;
        }
        else if (count == 1)
        {
            ByteRotateCarryLeft1(p, ref dest);
            return;
        }

        uint buffer = dest;

        if (p.Flags.Carry)
            buffer |= 0x0100;

        uint b = buffer << count;
        uint c = buffer >>> (9 - count);
        buffer = b | c;

        dest = (byte)buffer;
        p.Flags.Carry = (buffer & 0x0100) != 0;
    }

    [Opcode("D1/2 rmw", AddressSize = 16 | 32)]
    public static void WordRotateCarryLeft1(Processor p, ref ushort dest)
    {
        uint buffer = dest;

        if (p.Flags.Carry)
            buffer |= 0x00010000;

        uint b = (uint)(buffer << 1);
        uint c = (uint)(buffer >> 16);
        buffer = b | c;

        dest = (ushort)buffer;
        p.Flags.Carry = (buffer & 0x00010000) != 0;
        p.Flags.Overflow = (buffer & 0x00018000) == 0x00010000 || (buffer & 0x00018000) == 0x00008000;
    }
    [Alternate(nameof(WordRotateCarryLeft1), AddressSize = 16 | 32)]
    public static void DWordRotateCarryLeft1(Processor p, ref uint dest)
    {
        ulong buffer = dest;

        if (p.Flags.Carry)
            buffer |= 0x0000000100000000;

        ulong b = buffer << 1;
        ulong c = buffer >>> 32;
        buffer = b | c;

        dest = (uint)buffer;
        p.Flags.Carry = (buffer & 0x0000000100000000) != 0;
        p.Flags.Overflow = (buffer & 0x0000000180000000) == 0x0000000100000000 || (buffer & 0x0000000180000000) == 0x0000000080000000;
    }

    [Opcode("D3/2 rmw,cl|C1/2 rmw,ib", AddressSize = 16 | 32)]
    public static void WordRotateCarryLeft(Processor p, ref ushort dest, byte count)
    {
        count = (byte)((count & 0x1F) % 17);
        if (count == 0)
        {
            return;
        }
        else if (count == 1)
        {
            WordRotateCarryLeft1(p, ref dest);
            return;
        }

        uint buffer = dest;

        if (p.Flags.Carry)
            buffer |= 0x00010000;

        uint b = buffer << count;
        uint c = buffer >>> (17 - count);
        buffer = b | c;

        dest = (ushort)buffer;
        p.Flags.Carry = (buffer & 0x00010000) != 0;
    }
}
