using System.Numerics;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.BitwiseLogic;

internal static class Bsr
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("0FBD/r rw,rmw", OperandSize = 16, AddressSize = 16 | 32)]
    public static void BitScanReverse16(Processor p, ref ushort index, ushort value)
    {
        p.Flags.Zero = index == 0;
        index = (ushort)(31 - BitOperations.LeadingZeroCount(value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Alternate(nameof(BitScanReverse16), OperandSize = 32, AddressSize = 16 | 32)]
    public static void BitScanReverse32(Processor p, ref uint index, uint value)
    {
        p.Flags.Zero = index == 0;
        index = (uint)(31 - BitOperations.LeadingZeroCount(value));
    }
}
