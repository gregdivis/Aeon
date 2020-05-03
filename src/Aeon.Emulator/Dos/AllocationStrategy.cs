namespace Aeon.Emulator.Dos
{
    /// <summary>
    /// Allocation strategy used by the DOS memory allocator.
    /// </summary>
    internal enum AllocationStrategy
    {
        /// <summary>
        /// Allocate the first available block that fits.
        /// </summary>
        LowFirstFit = 0x00,
        /// <summary>
        /// Allocate the best-fitting available block.
        /// </summary>
        LowBestFit = 0x01,
        /// <summary>
        /// Allocate the last available block that fits.
        /// </summary>
        LowLastFit = 0x02,
        /// <summary>
        /// Allocate the first available high memory block that fits.
        /// </summary>
        HighFirstFit = 0x40,
        /// <summary>
        /// Allocate the best-fitting available high memory block.
        /// </summary>
        HighBestFit = 0x41,
        /// <summary>
        /// Allocate the last available high memory block that fits.
        /// </summary>
        HighLastFit = 0x42,
        /// <summary>
        /// Allocate the first available block that fits in high or low memory.
        /// </summary>
        HighLowFirstFit = 0x80,
        /// <summary>
        /// Allocate the best-fitting available block in high or low memory.
        /// </summary>
        HighLowBestFit = 0x81,
        /// <summary>
        /// Allocate the last available block that fits in high or low memory.
        /// </summary>
        HighLowLastFit = 0x82
    }
}
