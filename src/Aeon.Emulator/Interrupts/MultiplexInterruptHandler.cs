using System.Collections.Generic;

#nullable disable

namespace Aeon.Emulator.Interrupts
{
    /// <summary>
    /// Emulates the 2F multiplex interrupt.
    /// </summary>
    internal sealed class MultiplexInterruptHandler : IInterruptHandler
    {
        private Processor processor;
        private readonly List<IMultiplexInterruptHandler> handlers = new();

        IEnumerable<InterruptHandlerInfo> IInterruptHandler.HandledInterrupts => new InterruptHandlerInfo[] { 0x2F };
        public IList<IMultiplexInterruptHandler> Handlers => this.handlers;

        void IInterruptHandler.HandleInterrupt(int interrupt)
        {
            int id = this.processor.AH;

            foreach (var handler in this.handlers)
            {
                if (handler.Identifier == id)
                {
                    handler.HandleInterrupt();
                    return;
                }
            }

            System.Diagnostics.Debug.WriteLine($"Multiplex interrupt ID {id:X2}h not implemented.");
        }
        void IVirtualDevice.DeviceRegistered(VirtualMachine vm) => this.processor = vm.Processor;
    }
}
