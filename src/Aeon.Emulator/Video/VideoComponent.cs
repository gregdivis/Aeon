using System;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Video
{
    internal abstract class VideoComponent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VideoComponent"/> class.
        /// </summary>
        protected VideoComponent()
        {
        }

        /// <summary>
        /// Expands the low four bits of a value into four bytes in an array.
        /// </summary>
        /// <param name="value">Value to expand.</param>
        /// <param name="array">Array where four expanded values are written.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExpandRegister(byte value, Span<byte> array)
        {
            array[0] = 0;
            array[1] = 0;
            array[2] = 0;
            array[3] = 0;
            if ((value & 0x01) != 0)
                array[0] = 0xFF;
            if ((value & 0x02) != 0)
                array[1] = 0xFF;
            if ((value & 0x04) != 0)
                array[2] = 0xFF;
            if ((value & 0x08) != 0)
                array[3] = 0xFF;
        }
        /// <summary>
        /// Expands the low four bits of a value into four bytes in an array.
        /// </summary>
        /// <param name="value">Value to expand.</param>
        /// <param name="array">Array where four expanded values are written.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExpandRegister(byte value, Span<bool> array)
        {
            array[0] = (value & 0x01) != 0;
            array[1] = (value & 0x02) != 0;
            array[2] = (value & 0x04) != 0;
            array[3] = (value & 0x08) != 0;
        }
    }
}
