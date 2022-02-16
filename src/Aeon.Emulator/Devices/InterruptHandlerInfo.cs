namespace Aeon.Emulator
{
    /// <summary>
    /// Describes an interrupt handled by a virtual device.
    /// </summary>
    /// <param name="Interrupt">The handled interrupt.</param>
    /// <param name="SavedRegisters">The registers to be saved before the handler is invoked.</param>
    /// <param name="IsHookable">Value indicating whether the interrupt handler is hookable.</param>
    /// <param name="ClearInterruptFlag">Value indicating whether the CPU Interrupt Enable flag should be cleared by the interrupt handler.</param>
    public readonly record struct InterruptHandlerInfo(byte Interrupt, Registers SavedRegisters = Registers.None, bool IsHookable = false, bool ClearInterruptFlag = false)
    {
        public static implicit operator InterruptHandlerInfo(byte interrupt) => new(interrupt);

        public override string ToString() => $"int {this.Interrupt:X2}h, {this.SavedRegisters}";
    }
}
