using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.BitShifting
{
    internal static class Sar
    {
        [Opcode("D0/7 rmb", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ByteArithmeticShiftRight1(Processor p, ref sbyte dest)
        {
            byte value = (byte)dest;
            dest >>= 1;
            p.Flags.Update_Sar1_Byte(value, (byte)dest);
        }
        [Opcode("D2/7 rmb,cl|C0/7 rmb,ib", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ByteArithmeticShiftRight(Processor p, ref sbyte dest, byte count)
        {
            count &= 0x1F;
            if (count == 0)
            {
                return;
            }
            else if (count == 1)
            {
                ByteArithmeticShiftRight1(p, ref dest);
                return;
            }

            byte value = (byte)dest;
            dest >>= count;
            p.Flags.Update_Sar_Byte(value, count, (byte)dest);
        }

        [Opcode("D1/7 rmw", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WordArithmeticShiftRight1(Processor p, ref short dest)
        {
            ushort value = (ushort)dest;
            dest >>= 1;
            p.Flags.Update_Sar1_Word(value, (ushort)dest);
        }
        [Opcode("D3/7 rmw,cl|C1/7 rmw,ib", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WordArithmeticShiftRight(Processor p, ref short dest, byte count)
        {
            count &= 0x1F;
            if (count == 0)
            {
                return;
            }
            else if (count == 1)
            {
                WordArithmeticShiftRight1(p, ref dest);
                return;
            }

            ushort value = (ushort)dest;
            dest >>= count;
            p.Flags.Update_Sar_Word(value, count, (ushort)dest);
        }

        [Alternate(nameof(WordArithmeticShiftRight1), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DWordArithmeticShiftRight1(Processor p, ref int dest)
        {
            uint value = (uint)dest;
            dest >>= 1;
            p.Flags.Update_Sar1_DWord(value, (uint)dest);
        }
        [Alternate(nameof(WordArithmeticShiftRight), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DWordArithmeticShiftRight(Processor p, ref int dest, byte count)
        {
            count &= 0x1F;
            if (count == 0)
            {
                return;
            }
            else if (count == 1)
            {
                DWordArithmeticShiftRight1(p, ref dest);
                return;
            }

            uint value = (uint)dest;
            dest >>= count;
            p.Flags.Update_Sar_DWord(value, count, (uint)dest);
        }
    }
}
