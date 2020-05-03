using System;

namespace Aeon.Emulator
{
    /// <summary>
    /// Defines a device which supports 8-bit DMA transfers.
    /// </summary>
    public interface IDmaDevice8 : IVirtualDevice
    {
        /// <summary>
        /// Gets the DMA channel of the device.
        /// </summary>
        int Channel { get; }

        /// <summary>
        /// Writes bytes of data to the DMA device.
        /// </summary>
        /// <param name="source">Address of first byte to write to the device.</param>
        /// <param name="count">Number of bytes to write.</param>
        /// <returns>Number of bytes actually written to the device.</returns>
        int WriteBytes(IntPtr source, int count);
        /// <summary>
        /// Invoked when a transfer is completed in single-cycle mode.
        /// </summary>
        void SingleCycleComplete();
    }
}
