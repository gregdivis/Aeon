using System.Numerics;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.BitwiseLogic
{
    internal static class Bsf
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Opcode("0FBC/r rw,rmw", OperandSize = 16, AddressSize = 16 | 32)]
        public static void BitScanForward16(Processor p, ref ushort index, ushort value)
        {
            int tzcnt = BitOperations.TrailingZeroCount(value);
            if (tzcnt < 16)
            {
                index = (ushort)tzcnt;
                p.Flags.Zero = false;
            }
            else
            {
                p.Flags.Zero = true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Alternate(nameof(BitScanForward16), OperandSize = 32, AddressSize = 16 | 32)]
        public static void BitScanReverse32(Processor p, ref uint index, uint value)
        {
            int tzcnt = BitOperations.TrailingZeroCount(value);
            if (tzcnt < 32)
            {
                index = (uint)tzcnt;
                p.Flags.Zero = false;
            }
            else
            {
                p.Flags.Zero = true;
            }
        }
    }
}
