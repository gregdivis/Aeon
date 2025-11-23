namespace Aeon.Emulator;

/// <summary>
/// Defines an interrupt handler for a virtual machine.
/// </summary>
public interface IInterruptHandler : IVirtualDevice
{
    /// <summary>
    /// Gets the interrupts which are handled by this handler.
    /// </summary>
    IEnumerable<InterruptHandlerInfo> HandledInterrupts { get; }

    /// <summary>
    /// Called when the interrupt handler should perform its action.
    /// </summary>
    /// <param name="interrupt">Raised interrupt number.</param>
    void HandleInterrupt(int interrupt);
}
