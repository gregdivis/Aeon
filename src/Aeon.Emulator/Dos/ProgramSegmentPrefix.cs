using System.Runtime.InteropServices;

namespace Aeon.Emulator.Dos
{
    /// <summary>
    /// In-memory representation of a DOS program segment prefix.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    internal struct ProgramSegmentPrefix
    {
        /// <summary>
        /// Returns a new PSP with its constant fields initialized.
        /// </summary>
        /// <returns>New PSP with its constant fields initialized.</returns>
        public static ProgramSegmentPrefix CreateDefault()
        {
            var psp = new ProgramSegmentPrefix
            {
                Int20h = 0x20CD,
                Int21h = 0x21CD,
                Retf = 0xCB,
                MaxFileHandles = 32
            };

            return psp;
        }

        /// <summary>
        /// INT 20h instruction. This field is constant.
        /// </summary>
        [FieldOffset(0x00)]
        public ushort Int20h;
        /// <summary>
        /// Last paragraph allocated to the process.
        /// </summary>
        [FieldOffset(0x02)]
        public ushort EndAddress;

        /// <summary>
        /// Offset of the previous termination handler address.
        /// </summary>
        [FieldOffset(0x0A)]
        public ushort ProgramTerminationOffset;
        /// <summary>
        /// Segment of the previous termination handler address.
        /// </summary>
        [FieldOffset(0x0C)]
        public ushort ProgramTerminationSegment;

        /// <summary>
        /// Offset of the break handler address.
        /// </summary>
        [FieldOffset(0x0E)]
        public ushort BreakHandlerOffset;
        /// <summary>
        /// Segment of the break handler address.
        /// </summary>
        [FieldOffset(0x10)]
        public ushort BreakHandlerSegment;

        /// <summary>
        /// Offset of the critical error handler address.
        /// </summary>
        [FieldOffset(0x12)]
        public ushort CriticalErrorHandlerOffset;
        /// <summary>
        /// Segment of the critical error handler address.
        /// </summary>
        [FieldOffset(0x14)]
        public ushort CriticalErrorHandlerSegment;

        /// <summary>
        /// PSP segment of the parent process;
        /// </summary>
        [FieldOffset(0x16)]
        public ushort ParentProcessId;

        /// <summary>
        /// The process environment string segment.
        /// </summary>
        [FieldOffset(0x2C)]
        public ushort EnvironmentSegment;

        /// <summary>
        /// The maximum number of file handles for the process.
        /// </summary>
        [FieldOffset(0x32)]
        public ushort MaxFileHandles;

        /// <summary>
        /// The offset of the process's handle table.
        /// </summary>
        [FieldOffset(0x34)]
        public ushort HandleTableOffset;
        /// <summary>
        /// The segment of the process's handle table.
        /// </summary>
        [FieldOffset(0x36)]
        public ushort HandleTableSegment;

        /// <summary>
        /// INT 21h instruction. This field is constant.
        /// </summary>
        [FieldOffset(0x50)]
        public ushort Int21h;
        /// <summary>
        /// RETF instruction. This field is constant.
        /// </summary>
        [FieldOffset(0x52)]
        public byte Retf;

        /// <summary>
        /// The length of the command line string.
        /// </summary>
        [FieldOffset(0x80)]
        public byte CommandLineLength;
        /// <summary>
        /// The command line string.
        /// </summary>
        [FieldOffset(0x81)]
        public unsafe fixed byte CommandLine[127];
    }
}
