using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.Arithmetic
{
    internal static class Sub
    {
        [Opcode("2C al,ib|80/5 rmb,ib|28/r rmb,rb|2A/r rb,rmb", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ByteSubtract(Processor p, ref byte dest, byte src)
        {
            byte uResult = (byte)((uint)dest - (uint)src);
            p.Flags.Update_Sub_Byte(dest, src, uResult);
            dest = uResult;
        }

        [Opcode("2D ax,iw|81/5 rmw,iw|83/5 rmw,ibx|29/r rmw,rw|2B/r rw,rmw", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WordSubtract(Processor p, ref ushort dest, ushort src)
        {
            ushort uResult = (ushort)((uint)dest - (uint)src);
            p.Flags.Update_Sub_Word(dest, src, uResult);
            dest = uResult;
        }
        [Alternate("WordSubtract", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DWordSubtract(Processor p, ref uint dest, uint src)
        {
            uint uResult = dest - src;
            p.Flags.Update_Sub_DWord(dest, src, uResult);
            dest = uResult;
        }
    }

    internal static class Sbb
    {
        [Opcode("1C al,ib|80/3 rmb,ib|18/r rmb,rb|1A/r rb,rmb", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ByteCarrySub(Processor p, ref byte dest, byte src)
        {
            uint c = p.Flags.Carry ? 1u : 0u;
            uint uResult = (uint)dest - (uint)src - c;
            p.Flags.Update_Sbb_Byte(dest, src, c, (byte)uResult);
            dest = (byte)(uResult & 0xFFu);
        }

        [Opcode("1D ax,iw|81/3 rmw,iw|83/3 rmw,ibx|19/r rmw,rw|1B/r rw,rmw", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WordCarrySub(Processor p, ref ushort dest, ushort src)
        {
            uint c = p.Flags.Carry ? 1u : 0u;
            uint uResult = (uint)dest - (uint)src - c;
            p.Flags.Update_Sbb_Word(dest, src, c, (ushort)uResult);
            dest = (ushort)(uResult & 0xFFFFu);
        }
        [Alternate("WordCarrySub", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DWordCarrySub(Processor p, ref uint dest, uint src)
        {
            uint c = p.Flags.Carry ? 1u : 0u;
            ulong uResult = (ulong)dest - (ulong)src - (ulong)c;
            p.Flags.Update_Sbb_DWord(dest, src, c, (uint)uResult);
            dest = (uint)(uResult & 0xFFFFFFFFu);
        }
    }

    internal static class Dec
    {
        [Opcode("FE/1 rmb", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ByteDecrement(Processor p, ref byte dest)
        {
            uint uResult = (uint)dest - 1u;
            p.Flags.Update_Dec_Byte(dest, (byte)uResult);
            dest = (byte)uResult;
        }

        [Opcode("48+ rw|FF/1 rmw", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WordDecrement(Processor p, ref ushort dest)
        {
            uint uResult = (uint)dest - 1u;
            p.Flags.Update_Dec_Word(dest, (ushort)uResult);
            dest = (ushort)uResult;
        }
        [Alternate(nameof(WordDecrement), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DWordDecrement(Processor p, ref uint dest)
        {
            uint uResult = dest - 1u;
            p.Flags.Update_Dec_DWord(dest, uResult);
            dest = uResult;
        }
    }

    internal static class Neg
    {
        [Opcode("F6/3 rmb", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ByteNegate(Processor p, ref sbyte dest)
        {
            sbyte sResult = (sbyte)-dest;
            p.Flags.Update_Sub_Byte(0, (byte)dest, (byte)sResult);
            dest = sResult;
        }

        [Opcode("F7/3 rmw", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WordNegate(Processor p, ref short dest)
        {
            short sResult = (short)-dest;
            p.Flags.Update_Sub_Word(0, (ushort)dest, (ushort)sResult);
            dest = sResult;
        }
        [Alternate(nameof(WordNegate), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DWordNegate(Processor p, ref int dest)
        {
            int sResult = -dest;
            p.Flags.Update_Sub_DWord(0, (uint)dest, (uint)sResult);
            dest = sResult;
        }
    }
}
