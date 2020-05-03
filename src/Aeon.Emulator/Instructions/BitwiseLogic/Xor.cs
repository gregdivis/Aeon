using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.BitwiseLogic
{
    internal static class Xor
    {
        [Opcode("34 al,ib|80/6 rmb,ib|30/r rmb,rb|32/r rb,rmb", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ByteXor(Processor p, ref byte dest, byte src)
        {
            dest = (byte)(dest ^ src);
            p.Flags.Update_Value_Byte(dest);
            p.Flags.Carry = false;
            p.Flags.Overflow = false;
        }

        [Opcode("35 ax,iw|81/6 rmw,iw|83/6 rmw,ibx|31/r rmw,rw|33/r rw,rmw", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WordXor(Processor p, ref ushort dest, ushort src)
        {
            dest = (ushort)(dest ^ src);
            p.Flags.Update_Value_Word(dest);
            p.Flags.Carry = false;
            p.Flags.Overflow = false;
        }
        [Alternate("WordXor", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DWordXor(Processor p, ref uint dest, uint src)
        {
            dest = (uint)(dest ^ src);
            p.Flags.Update_Value_DWord(dest);
            p.Flags.Carry = false;
            p.Flags.Overflow = false;
        }
    }
}
