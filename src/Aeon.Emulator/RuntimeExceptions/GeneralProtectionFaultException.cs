namespace Aeon.Emulator.RuntimeExceptions
{
    /// <summary>
    /// Represents an emulated general protection fault exception.
    /// </summary>
    public sealed class GeneralProtectionFaultException : EmulatedException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralProtectionFaultException"/> class.
        /// </summary>
        public GeneralProtectionFaultException()
            : base(0x0D, "General protection fault")
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralProtectionFaultException"/> class.
        /// </summary>
        /// <param name="errorCode">Fault error code.</param>
        public GeneralProtectionFaultException(int errorCode)
            : this()
        {
            this.ErrorCode = errorCode;
        }

        /// <summary>
        /// Gets the optional error code for the interrupt.
        /// </summary>
        public override int? ErrorCode { get; }
    }
}
