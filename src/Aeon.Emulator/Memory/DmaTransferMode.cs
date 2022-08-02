namespace Aeon.Emulator
{
    /// <summary>
    /// Specifies the transfer mode of a DMA channel.
    /// </summary>
    public enum DmaTransferMode
    {
        /// <summary>
        /// The DMA channel is in single-cycle mode.
        /// </summary>
        SingleCycle,
        /// <summary>
        /// The DMA channel is in auto-initialize mode.
        /// </summary>
        AutoInitialize
    }
}
