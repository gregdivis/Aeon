using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.BitShifting;

internal static class Rcr
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("D0/3 rmb", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void ByteRotateCarryRight1(Processor p, ref byte dest)
    {
        uint buffer = dest;

        if (p.Flags.Carry)
            buffer |= 0x0100;

        uint b = buffer >>> 1;
        uint c = buffer << 8;
        buffer = b | c;

        dest = (byte)buffer;
        p.Flags.Carry = (buffer & 0x0100) != 0;
        p.Flags.Overflow = (dest & 0xC0) == 0x80 || (dest & 0xC0) == 0x40;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("D2/3 rmb,cl|C0/3 rmb,ib", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void ByteRotateCarryRight(Processor p, ref byte dest, byte count)
    {
        count = (byte)((count & 0x1F) % 9);
        if (count == 0)
        {
            return;
        }
        else if (count == 1)
        {
            ByteRotateCarryRight1(p, ref dest);
            return;
        }

        uint buffer = dest;

        if (p.Flags.Carry)
            buffer |= 0x0100;

        uint b = buffer >>> count;
        uint c = buffer << (9 - count);
        buffer = b | c;

        dest = (byte)buffer;
        p.Flags.Carry = (buffer & 0x0100) != 0;
    }

    [Opcode("D1/3 rmw", AddressSize = 16 | 32)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WordRotateCarryRight1(Processor p, ref ushort dest)
    {
        uint buffer = dest;

        if (p.Flags.Carry)
            buffer |= 0x00010000;

        uint b = buffer >>> 1;
        uint c = buffer << 16;
        buffer = b | c;

        dest = (ushort)buffer;
        p.Flags.Carry = (buffer & 0x00010000) != 0;
        p.Flags.Overflow = (dest & 0xC000) == 0x8000 || (dest & 0xC000) == 0x4000;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("D3/3 rmw,cl|C1/3 rmw,ib", AddressSize = 16 | 32)]
    public static void WordRotateCarryRight(Processor p, ref ushort dest, byte count)
    {
        count = (byte)((count & 0x1F) % 17);
        if (count == 0)
        {
            return;
        }
        else if (count == 1)
        {
            WordRotateCarryRight1(p, ref dest);
            return;
        }

        uint buffer = dest;

        if (p.Flags.Carry)
            buffer |= 0x00010000;

        uint b = buffer >>> count;
        uint c = buffer << (17 - count);
        buffer = b | c;

        dest = (ushort)buffer;
        p.Flags.Carry = (buffer & 0x00010000) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Alternate(nameof(WordRotateCarryRight1), AddressSize = 16 | 32)]
    public static void DWordRotateCarryRight1(Processor p, ref uint dest)
    {
        ulong buffer = dest;

        if (p.Flags.Carry)
            buffer |= 0x100000000;

        ulong b = buffer >>> 1;
        ulong c = buffer << 32;
        buffer = b | c;

        dest = (uint)buffer;
        p.Flags.Carry = (buffer & 0x100000000) != 0;
        p.Flags.Overflow = (dest & 0xC0000000) == 0x80000000 || (dest & 0xC0000000) == 0x40000000;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Alternate(nameof(WordRotateCarryRight), AddressSize = 16 | 32)]
    public static void DWordRotateCarryRight(Processor p, ref uint dest, byte count)
    {
        count = (byte)((count & 0x1F) % 33);
        if (count == 0)
        {
            return;
        }
        else if (count == 1)
        {
            DWordRotateCarryRight1(p, ref dest);
            return;
        }

        ulong buffer = dest;

        if (p.Flags.Carry)
            buffer |= 0x100000000;

        ulong b = buffer >>> count;
        ulong c = buffer << (33 - count);
        buffer = b | c;

        dest = (uint)buffer;
        p.Flags.Carry = (buffer & 0x100000000) != 0;
    }
}
