namespace Aeon.Emulator.DebugSupport
{
    /// <summary>
    /// Describes an object which provides a view of memory.
    /// </summary>
    public interface IMemorySource
    {
        /// <summary>
        /// Reads bytes from memory into a buffer.
        /// </summary>
        /// <param name="buffer">Buffer into which bytes will be written.</param>
        /// <param name="bufferOffset">Offset in buffer to start writing.</param>
        /// <param name="address">Address in memory to start copying from.</param>
        /// <param name="count">Number of bytes to copy.</param>
        void ReadBytes(byte[] buffer, int bufferOffset, QualifiedAddress address, int count);
        /// <summary>
        /// Returns the logical address of a real-mode or protected-mode address.
        /// </summary>
        /// <param name="source">Real-mode or protected-mode address to resolve.</param>
        /// <returns>Logical address of the provided address or null if the address could not be resolved.</returns>
        QualifiedAddress? GetLogicalAddress(QualifiedAddress source);
        /// <summary>
        /// Returns the physical address of a real-mode, protected-mode, or logical address.
        /// </summary>
        /// <param name="source">Real-mode, protected-mode, or logical address to resolve.</param>
        /// <returns>Physical address of the provided address or null if the address could not be resolved.</returns>
        QualifiedAddress? GetPhysicalAddress(QualifiedAddress source);
    }
}
