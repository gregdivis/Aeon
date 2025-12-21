using System.Numerics;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator;

public static class Intrinsics
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ExtractBits(uint value, byte start, byte length, uint mask) => (value & mask) >>> start;
    /// <summary>
    /// Returns <paramref name="a"/> &amp; ~<paramref name="b"/>.
    /// </summary>
    /// <param name="a">The first value.</param>
    /// <param name="b">The second value.</param>
    /// <returns>The result of <paramref name="a"/> &amp; ~<paramref name="b"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint AndNot(uint a, uint b) => a & ~b;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ResetLowestSetBit(uint value)
    {
        int trailingZeroCount = BitOperations.TrailingZeroCount(value);
        return trailingZeroCount < 32 ? value & ~(1u << trailingZeroCount) : 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte HighByte(ushort value) => (byte)(value >>> 8);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte LowByte(ushort value) => (byte)value;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort HighWord(uint value) => (ushort)(value >>> 16);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort LowWord(uint value) => (ushort)value;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint HighDWord(ulong value) => (uint)(value >>> 32);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint LowDWord(ulong value) => (uint)value;
}
