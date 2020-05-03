using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.BitShifting
{
    internal static class Rcl
    {
        [Opcode("D0/2 rmb", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ByteRotateCarryLeft1(Processor p, ref byte dest)
        {
            uint buffer = dest;

            if (p.Flags.Carry)
                buffer |= 0x0100;

            uint b = (uint)(buffer << 1);
            uint c = (uint)(buffer >> 8);
            buffer = b | c;

            dest = (byte)buffer;
            p.Flags.Carry = (buffer & 0x0100) == 0x0100;
            p.Flags.Overflow = (buffer & 0x0180) == 0x0100 || (buffer & 0x0180) == 0x0080;
        }
        [Opcode("D2/2 rmb,cl|C0/2 rmb,ib", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

            uint b = (uint)(buffer << count);
            uint c = (uint)(buffer >> (9 - count));
            buffer = b | c;

            dest = (byte)buffer;
            p.Flags.Carry = (buffer & 0x0100) != 0;
        }

        [Opcode("D1/2 rmw", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DWordRotateCarryLeft1(Processor p, ref uint dest)
        {
            ulong buffer = dest;

            if (p.Flags.Carry)
                buffer |= 0x0000000100000000;

            ulong b = (ulong)(buffer << 1);
            ulong c = (ulong)(buffer >> 32);
            buffer = b | c;

            dest = (uint)buffer;
            p.Flags.Carry = (buffer & 0x0000000100000000) != 0;
            p.Flags.Overflow = (buffer & 0x0000000180000000) == 0x0000000100000000 || (buffer & 0x0000000180000000) == 0x0000000080000000;
        }

        [Opcode("D3/2 rmw,cl|C1/2 rmw,ib", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

            uint b = (uint)(buffer << count);
            uint c = (uint)(buffer >> (17 - count));
            buffer = b | c;

            dest = (ushort)buffer;
            p.Flags.Carry = (buffer & 0x00010000) != 0;
        }
    }

    internal static class Rcr
    {
        [Opcode("D0/3 rmb", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ByteRotateCarryRight1(Processor p, ref byte dest)
        {
            uint buffer = dest;

            if (p.Flags.Carry)
                buffer |= 0x0100;

            uint b = (uint)(buffer >> 1);
            uint c = (uint)(buffer << 8);
            buffer = b | c;

            dest = (byte)buffer;
            p.Flags.Carry = (buffer & 0x0100) != 0;
            p.Flags.Overflow = (dest & 0xC0) == 0x80 || (dest & 0xC0) == 0x40;
        }
        [Opcode("D2/3 rmb,cl|C0/3 rmb,ib", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

            uint b = (uint)(buffer >> count);
            uint c = (uint)(buffer << (9 - count));
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

            uint b = (uint)(buffer >> 1);
            uint c = (uint)(buffer << 16);
            buffer = b | c;

            dest = (ushort)buffer;
            p.Flags.Carry = (buffer & 0x00010000) != 0;
            p.Flags.Overflow = (dest & 0xC000) == 0x8000 || (dest & 0xC000) == 0x4000;
        }
        [Opcode("D3/3 rmw,cl|C1/3 rmw,ib", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

            uint b = (uint)(buffer >> count);
            uint c = (uint)(buffer << (17 - count));
            buffer = b | c;

            dest = (ushort)buffer;
            p.Flags.Carry = (buffer & 0x00010000) != 0;
        }

        [Alternate(nameof(WordRotateCarryRight1), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DWordRotateCarryRight1(Processor p, ref uint dest)
        {
            ulong buffer = dest;

            if (p.Flags.Carry)
                buffer |= 0x100000000;

            ulong b = (ulong)(buffer >> 1);
            ulong c = (ulong)(buffer << 32);
            buffer = b | c;

            dest = (uint)buffer;
            p.Flags.Carry = (buffer & 0x100000000) != 0;
            p.Flags.Overflow = (dest & 0xC0000000) == 0x80000000 || (dest & 0xC0000000) == 0x40000000;
        }
        [Alternate(nameof(WordRotateCarryRight), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

            ulong b = (ulong)(buffer >> count);
            ulong c = (ulong)(buffer << (33 - count));
            buffer = b | c;

            dest = (uint)buffer;
            p.Flags.Carry = (buffer & 0x100000000) != 0;
        }
    }
}
