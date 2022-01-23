using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;

namespace Aeon.Emulator
{
    public static class Intrinsics
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ExtractBits(uint value, byte start, byte length, uint mask)
        {
            if (Bmi1.IsSupported)
                return Bmi1.BitFieldExtract(value, start, length);
            else
                return (value & mask) >> start;
        }
        /// <summary>
        /// Returns <paramref name="a"/> &amp; ~<paramref name="b"/>.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <returns>The result of <paramref name="a"/> &amp; ~<paramref name="b"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint AndNot(uint a, uint b)
        {
            if (Bmi1.IsSupported)
                return Bmi1.AndNot(b, a);
            else
                return a & ~b;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort HighWord(uint value)
        {
            unsafe
            {
                return ((ushort*)&value)[1];
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort LowWord(uint value) => (ushort)value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint HighDWord(ulong value)
        {
            unsafe
            {
                return ((uint*)&value)[1];
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint LowDWord(ulong value) => (uint)value;
    }
}
