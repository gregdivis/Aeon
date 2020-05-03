namespace Aeon.Emulator
{
    /// <summary>
    /// Defines an interrupt handler for a virtual machine on the 2F multiplex interrupt.
    /// </summary>
    public interface IMultiplexInterruptHandler : IVirtualDevice
    {
        /// <summary>
        /// Gets the interrupt handler's identifier selected by the value in the AH register.
        /// </summary>
        int Identifier { get; }

        /// <summary>
        /// Called when the interrupt handler should perform its action.
        /// </summary>
        void HandleInterrupt();
    }
}
