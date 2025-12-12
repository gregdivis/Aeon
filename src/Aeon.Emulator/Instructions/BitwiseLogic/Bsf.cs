using System.Numerics;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.BitwiseLogic;

internal static class Bsf
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("0FBC/r rw,rmw", OperandSize = 16, AddressSize = 16 | 32)]
    public static void BitScanForward16(Processor p, ref ushort index, ushort value)
    {
        p.Flags.Zero = index == 0;
        index = (ushort)BitOperations.TrailingZeroCount(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Alternate(nameof(BitScanForward16), OperandSize = 32, AddressSize = 16 | 32)]
    public static void BitScanReverse32(Processor p, ref uint index, uint value)
    {
        p.Flags.Zero = index == 0;
        index = (uint)BitOperations.TrailingZeroCount(value);
    }
}
