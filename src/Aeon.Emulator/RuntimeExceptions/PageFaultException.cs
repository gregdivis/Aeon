namespace Aeon.Emulator.RuntimeExceptions
{
    /// <summary>
    /// Represents an emulated page fault exception.
    /// </summary>
    public sealed class PageFaultException : EmulatedException
    {
        private bool userMode;

        /// <summary>
        /// Initializes a new instance of the <see cref="PageFaultException"/> class.
        /// </summary>
        /// <param name="faultAddress">Address which caused the page fault.</param>
        /// <param name="cause">Type of operation which cause the page fault.</param>
        public PageFaultException(uint faultAddress, PageFaultCause cause)
            : base(0x0E, "Page fault")
        {
            this.FaultAddress = faultAddress;
            this.Cause = cause;
        }

        /// <summary>
        /// Gets the address which caused the page fault.
        /// </summary>
        public uint FaultAddress { get; }
        /// <summary>
        /// Gets the type of operation which caused the page fault.
        /// </summary>
        public PageFaultCause Cause { get; }
        /// <summary>
        /// Gets the optional error code for the interrupt.
        /// </summary>
        public override int? ErrorCode
        {
            get
            {
                int errorCode = 0;
                if (this.Cause == PageFaultCause.Write)
                    errorCode |= (1 << 1);
                //else if(this.Cause == PageFaultCause.InstructionFetch)
                //    errorCode |= (1 << 4);

                if (this.userMode)
                    errorCode |= (1 << 2);

                return errorCode;
            }
        }

        /// <summary>
        /// Invoked when the exception is raised by the emulator.
        /// </summary>
        /// <param name="vm">VirtualMachine instance which is raising the exception.</param>
        internal override void OnRaised(VirtualMachine vm)
        {
            uint cpl = vm.Processor.CPL;
            this.userMode = cpl != 0;
            vm.Processor.CR2 = this.FaultAddress;
            System.Diagnostics.Debug.WriteLine($"Fault address: {this.FaultAddress:X8}, Code: {this.ErrorCode:X2}");
        }
    }

    /// <summary>
    /// Specifies the type of operation which caused a page fault.
    /// </summary>
    public enum PageFaultCause
    {
        /// <summary>
        /// An invalid address was read.
        /// </summary>
        Read,
        /// <summary>
        /// An invalid address was written to.
        /// </summary>
        Write,
        /// <summary>
        /// An instruction was fetched from an invalid address.
        /// </summary>
        InstructionFetch
    }
}
