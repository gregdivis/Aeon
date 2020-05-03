using System;

namespace Aeon.Emulator.RuntimeExceptions
{
    /// <summary>
    /// Represents an exception that occurred in an emulated system.
    /// </summary>
    public class EmulatedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmulatedException"/> class.
        /// </summary>
        public EmulatedException()
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="EmulatedException"/> class.
        /// </summary>
        /// <param name="interrupt">Interupt to be raised on the emulated system.</param>
        public EmulatedException(int interrupt)
        {
            this.Interrupt = interrupt;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="EmulatedException"/> class.
        /// </summary>
        /// <param name="message">Message describing the exception.</param>
        public EmulatedException(string message)
            : base(message)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="EmulatedException"/> class.
        /// </summary>
        /// <param name="interrupt">Interupt to be raised on the emulated system.</param>
        /// <param name="message">Message describing the exception.</param>
        public EmulatedException(int interrupt, string message)
            : base(message)
        {
            this.Interrupt = interrupt;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="EmulatedException"/> class.
        /// </summary>
        /// <param name="message">Message describing the exception.</param>
        /// <param name="inner">Exception which caused this exception.</param>
        public EmulatedException(string message, Exception inner)
            : base(message, inner)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="EmulatedException"/> class.
        /// </summary>
        /// <param name="interrupt">Interupt to be raised on the emulated system.</param>
        /// <param name="message">Message describing the exception.</param>
        /// <param name="inner">Exception which caused this exception.</param>
        public EmulatedException(int interrupt, string message, Exception inner)
            : base(message, inner)
        {
            this.Interrupt = interrupt;
        }

        /// <summary>
        /// Gets the interrupt that should be raised on the emulated
        /// system as a result of the exception.
        /// </summary>
        public virtual int Interrupt { get; } = -1;
        /// <summary>
        /// Gets the optional error code for the interrupt.
        /// </summary>
        public virtual int? ErrorCode => null;

        /// <summary>
        /// Invoked when the exception is raised by the emulator.
        /// </summary>
        /// <param name="vm">VirtualMachine instance which is raising the exception.</param>
        internal virtual void OnRaised(VirtualMachine vm)
        {
        }
    }
}
