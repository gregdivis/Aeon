namespace Aeon.Emulator.Interrupts;

/// <summary>
/// Provides default handling of interrupts which signal error conditions (00h, 04h, 06h).
/// </summary>
internal sealed class ErrorHandler : IInterruptHandler
{
    IEnumerable<InterruptHandlerInfo> IInterruptHandler.HandledInterrupts => [0x00, 0x04, 0x06];

    void IInterruptHandler.HandleInterrupt(int interrupt)
    {
        switch (interrupt)
        {
            case 0x00:
                throw new InvalidOperationException("Unhandled divide-by-zero.");

            case 0x04:
                throw new InvalidOperationException("Unhandled numeric overflow.");

            case 0x06:
                throw new InvalidOperationException("Unhandled undefined opcode.");
        }
    }
}
