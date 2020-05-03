namespace Aeon.Emulator.RuntimeExceptions
{
    /// <summary>
    /// Represents an emulated segment not present exception.
    /// </summary>
    public class SegmentNotPresentException : EmulatedException
    {
        private readonly uint errorCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="SegmentNotPresentException"/> class.
        /// </summary>
        /// <param name="errorCode">The selector error code.</param>
        public SegmentNotPresentException(uint errorCode)
            : base(0xB, "Segment not present")
        {
            this.errorCode = errorCode;
        }

        /// <summary>
        /// Gets the exception's error code.
        /// </summary>
        public override int? ErrorCode => (int)this.errorCode;
    }

    /// <summary>
    /// Represents an emulated GDT segment not present exception.
    /// </summary>
    public class GDTSegmentNotPresentException : SegmentNotPresentException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GDTSegmentNotPresentException"/> class.
        /// </summary>
        /// <param name="selectorIndex">Index of the selector in the GDT.</param>
        public GDTSegmentNotPresentException(uint selectorIndex)
            : base(selectorIndex << 3)
        {
        }
    }

    /// <summary>
    /// Represents an emulated LDT segment not present exception.
    /// </summary>
    public class LDTSegmentNotPresentException : SegmentNotPresentException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LDTSegmentNotPresentException"/> class.
        /// </summary>
        /// <param name="selectorIndex">Index of the selector in the LDT.</param>
        public LDTSegmentNotPresentException(uint selectorIndex)
            : base((selectorIndex << 3) | 0x4u)
        {
        }
    }
}
