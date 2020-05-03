using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.BitwiseLogic
{
    internal static class Not
    {
        [Opcode("F6/2 rmb", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ByteNot(VirtualMachine vm, ref byte dest)
        {
            dest = (byte)~dest;
        }

        [Opcode("F7/2 rmw", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WordNot(VirtualMachine vm, ref ushort dest)
        {
            dest = (ushort)~dest;
        }
        [Alternate("WordNot", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DWordNot(VirtualMachine vm, ref uint dest)
        {
            dest = (uint)~dest;
        }
    }
}
