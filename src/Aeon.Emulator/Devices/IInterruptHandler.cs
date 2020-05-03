using System.Collections.Generic;

namespace Aeon.Emulator
{
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

    /// <summary>
    /// Describes an interrupt handled by a virtual device.
    /// </summary>
    public readonly struct InterruptHandlerInfo
    {
        /// <summary>
        /// Initializes a new InterruptHandlerInfo struct.
        /// </summary>
        /// <param name="interrupt">Interrupt which is handled.</param>
        /// <param name="savedRegisters">Registers to be saved before the handler is invoked.</param>
        /// <param name="isHookable">Value indicating whether the interrupt handler is hookable.</param>
        /// <param name="clearInterruptFlag">Value indicating whether the interrupt handler should clear the CPU Interrupt Enable flag.</param>
        public InterruptHandlerInfo(int interrupt, Registers savedRegisters, bool isHookable, bool clearInterruptFlag)
        {
            this.Interrupt = interrupt;
            this.SavedRegisters = savedRegisters;
            this.IsHookable = isHookable;
            this.ClearInterruptFlag = clearInterruptFlag;
        }
        /// <summary>
        /// Initializes a new InterruptHandlerInfo struct.
        /// </summary>
        /// <param name="interrupt">Interrupt which is handled.</param>
        public InterruptHandlerInfo(int interrupt)
            : this(interrupt, Registers.None, false, false)
        {
        }
        /// <summary>
        /// Initializes a new InterruptHandlerInfo struct.
        /// </summary>
        /// <param name="interrupt">Interrupt which is handled.</param>
        /// <param name="savedRegisters">Registers to be saved before the handler is invoked.</param>
        public InterruptHandlerInfo(int interrupt, Registers savedRegisters)
            : this(interrupt, savedRegisters, false, false)
        {
        }
        /// <summary>
        /// Initializes a new InterruptHandlerInfo struct.
        /// </summary>
        /// <param name="interrupt">Interrupt which is handled.</param>
        /// <param name="isHookable">Value indicating whether the interrupt handler is hookable.</param>
        public InterruptHandlerInfo(int interrupt, bool isHookable)
            : this(interrupt, Registers.None, isHookable, false)
        {
        }
        /// <summary>
        /// Initializes a new InterruptHandlerInfo struct.
        /// </summary>
        /// <param name="interrupt">Interrupt which is handled.</param>
        /// <param name="isHookable">Value indicating whether the interrupt handler is hookable.</param>
        /// <param name="clearInterruptFlag">Value indicating whether the interrupt handler should clear the CPU Interrupt Enable flag.</param>
        public InterruptHandlerInfo(int interrupt, bool isHookable, bool clearInterruptFlag)
            : this(interrupt, Registers.None, isHookable, clearInterruptFlag)
        {
        }

        public static implicit operator InterruptHandlerInfo(int interrupt) => new InterruptHandlerInfo(interrupt);

        /// <summary>
        /// Gets the handled interrupt.
        /// </summary>
        public int Interrupt { get; }
        /// <summary>
        /// Gets the registers to be saved before the handler is invoked.
        /// </summary>
        public Registers SavedRegisters { get; }
        /// <summary>
        /// Gets a value indicating whether the interrupt handler is hookable.
        /// </summary>
        public bool IsHookable { get; }
        /// <summary>
        /// Gets a value indicating whether the CPU Interrupt Enable flag should be cleared by the interrupt handler.
        /// </summary>
        public bool ClearInterruptFlag { get; }

        public override string ToString() => $"int {this.Interrupt:X2}h, {this.SavedRegisters}";
    }
}
