using System;
using System.Collections.Generic;
using System.IO;
using Aeon.Emulator;

namespace Aeon.Test
{
    public class CallbackInt : IInterruptHandler
    {
        private int interrupt;
        private Action callback;

        public CallbackInt(int interrupt, Action callback)
        {
            this.interrupt = interrupt;
            this.callback = callback;
        }

        public IEnumerable<InterruptHandlerInfo> HandledInterrupts
        {
            get { return new InterruptHandlerInfo[] { this.interrupt }; }
        }

        public void HandleInterrupt(int interrupt)
        {
            this.callback();
        }

        public void Pause()
        {
        }

        public void Resume()
        {
        }

        public void DeviceRegistered(VirtualMachine vm)
        {
        }

        public void Dispose()
        {
        }
    }
}
