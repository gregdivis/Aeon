namespace Aeon.Emulator.DebugSupport
{
    /// <summary>
    /// Describes an object containing machine code.
    /// </summary>
    public interface IMachineCodeSource
    {
        /// <summary>
        /// Returns the logical base address for a given selector.
        /// </summary>
        /// <param name="selector">Selector whose base address is returned.</param>
        /// <returns>Base address of the selector if it is valid; otherwise null.</returns>
        uint? GetBaseAddress(ushort selector);
        /// <summary>
        /// Reads 12 bytes of data from the machine code source at the specified address.
        /// </summary>
        /// <param name="buffer">Buffer into which data is read. Must be at least 12 bytes long.</param>
        /// <param name="logicalAddress">Logical address in machine code source where instruction is read from.</param>
        /// <returns>Number of bytes actually read. Should normally return 12.</returns>
        int ReadInstruction(byte[] buffer, uint logicalAddress);
    }
}
