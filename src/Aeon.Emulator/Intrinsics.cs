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
    }
}
