using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.Arithmetic
{
    internal static class Add
    {
        [Opcode("00/r rmb,rb|02/r rb,rmb|04 al,ib|80/0 rmb,ib", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ByteAdd(Processor p, ref byte dest, byte src)
        {
            uint uResult = (uint)dest + (uint)src;
            p.Flags.Update_Add_Byte(dest, src, (byte)uResult);
            dest = (byte)uResult;
        }
        
        [Opcode("01/r rmw,rw|03/r rw,rmw|05 ax,iw|81/0 rmw,iw|83/0 rmw,ibx", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WordAdd(Processor p, ref ushort dest, ushort src)
        {
            uint uResult = (uint)dest + (uint)src;
            p.Flags.Update_Add_Word(dest, src, (ushort)uResult);
            dest = (ushort)uResult;
        }
        [Alternate("WordAdd", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DWordAdd(Processor p, ref uint dest, uint src)
        {
            uint uResult = dest + src;
            p.Flags.Update_Add_DWord(dest, src, uResult);
            dest = uResult;
        }
    }

    internal static class Adc
    {
        [Opcode("10/r rmb,rb|12/r rb,rmb|14 al,ib|80/2 rmb,ib", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ByteCarryAdd(Processor p, ref byte dest, byte src)
        {
            uint c = p.Flags.Carry ? 1u : 0u;
            uint uResult = (uint)dest + (uint)src + c;
            p.Flags.Update_Adc_Byte(dest, src, c, (byte)uResult);
            dest = (byte)(uResult & 0xFFu);
        }

        [Opcode("11/r rmw,rw|13/r rw,rmw|15 ax,iw|81/2 rmw,iw|83/2 rmw,ibx", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WordCarryAdd(Processor p, ref ushort dest, ushort src)
        {
            uint c = p.Flags.Carry ? 1u : 0u;
            uint uResult = (uint)dest + (uint)src + c;
            p.Flags.Update_Adc_Word(dest, src, c, (ushort)uResult);
            dest = (ushort)(uResult & 0xFFFFu);
        }
        [Alternate("WordCarryAdd", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DWordCarryAdd(Processor p, ref uint dest, uint src)
        {
            uint c = p.Flags.Carry ? 1u : 0u;
            ulong uResult = (ulong)dest + (ulong)src + (ulong)c;
            p.Flags.Update_Adc_DWord(dest, src, c, (uint)uResult);
            dest = (uint)(uResult & 0xFFFFFFFFu);
        }
    }

    internal static class Inc
    {
        [Opcode("FE/0 rmb", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ByteIncrement(Processor p, ref byte dest)
        {
            uint uResult = (uint)dest + (uint)1;
            p.Flags.Update_Inc_Byte(dest, (byte)uResult);
            dest = (byte)(uResult & 0xFF);
        }

        [Opcode("40+ rw|FF/0 rmw", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WordIncrement(Processor p, ref ushort dest)
        {
            uint uResult = (uint)dest + (uint)1;
            p.Flags.Update_Inc_Word(dest, (ushort)uResult);
            dest = (ushort)(uResult & 0xFFFF);
        }
        [Alternate(nameof(WordIncrement), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DWordIncrement(Processor p, ref uint dest)
        {
            uint uResult = dest + 1U;
            p.Flags.Update_Inc_DWord(dest, uResult);
            dest = uResult;
        }
    }
}
