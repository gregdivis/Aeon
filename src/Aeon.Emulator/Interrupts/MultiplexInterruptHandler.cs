namespace Aeon.Emulator.Interrupts
{
    /// <summary>
    /// Emulates the 2F multiplex interrupt.
    /// </summary>
    internal sealed class MultiplexInterruptHandler(Processor processor) : IInterruptHandler
    {
        private readonly Processor processor = processor;

        IEnumerable<InterruptHandlerInfo> IInterruptHandler.HandledInterrupts => [0x2F];
        public List<IMultiplexInterruptHandler> Handlers { get; } = [];

        void IInterruptHandler.HandleInterrupt(int interrupt)
        {
            int id = this.processor.AH;

            foreach (var handler in this.Handlers)
            {
                if (handler.Identifier == id)
                {
                    handler.HandleInterrupt();
                    return;
                }
            }

            System.Diagnostics.Debug.WriteLine($"Multiplex interrupt ID {id:X2}h not implemented.");
        }
    }
}
