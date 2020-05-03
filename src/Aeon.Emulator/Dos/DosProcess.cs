using Aeon.Emulator.Dos.Programs;
using Aeon.Emulator.Dos.VirtualFileSystem;

namespace Aeon.Emulator.Dos
{
    /// <summary>
    /// Contains information about a DOS process.
    /// </summary>
    public sealed class DosProcess
    {
        /// <summary>
        /// Temporary process definition used for certain internal allocations.
        /// </summary>
        internal static readonly DosProcess NullProcess = new DosProcess(null, 1, 0, string.Empty);

        internal DosProcess(ProgramImage image, ushort pspSegment, ushort environmentSegment, string commandLineArgs)
        {
            if (image != null)
            {
                this.FullPath = image.FullPath;
                this.ImageName = image.FileName;
            }
            else
            {
                this.FullPath = VirtualPath.RelativeCurrent;
                this.ImageName = string.Empty;
            }
            this.PrefixSegment = pspSegment;
            this.EnvironmentSegment = environmentSegment;
            this.CommandLineArguments = commandLineArgs;

            // These get initialized to the command line data area.
            this.DiskTransferAreaSegment = pspSegment;
            this.DiskTransferAreaOffset = 0x80;
        }
        private DosProcess(VirtualPath fullPath, string imageName, ushort pspSegment, ushort environmentSegment, string commandLineArgs)
        {
            this.FullPath = fullPath;
            this.ImageName = imageName;
            this.PrefixSegment = pspSegment;
            this.EnvironmentSegment = environmentSegment;
            this.CommandLineArguments = commandLineArgs;
        }

        /// <summary>
        /// Gets the full path to the image that was loaded.
        /// </summary>
        public VirtualPath FullPath { get; }
        /// <summary>
        /// Gets the name of the process.
        /// </summary>
        public string ImageName { get; }
        /// <summary>
        /// Gets a string containing the arguments used to launch the process.
        /// </summary>
        public string CommandLineArguments { get; }

        /// <summary>
        /// Gets the segment of the process's PSP.
        /// </summary>
        internal ushort PrefixSegment { get; }
        /// <summary>
        /// Gets the segment of the process's environment block.
        /// </summary>
        internal ushort EnvironmentSegment { get; }
        /// <summary>
        /// Gets or sets the segment of the process disk transfer area.
        /// </summary>
        internal ushort DiskTransferAreaSegment { get; set; }
        /// <summary>
        /// Gets or sets the offset of the process disk transfer area.
        /// </summary>
        internal ushort DiskTransferAreaOffset { get; set; }
        /// <summary>
        /// Gets or sets a buffer containing the processor state before the process is started.
        /// </summary>
        internal byte[] InitialProcessorState { get; set; }

        /// <summary>
        /// Returns a short description of the process.
        /// </summary>
        /// <returns>Short description of the process.</returns>
        public override string ToString() => this.ImageName;

        internal DosProcess CreateChildProcess(ushort pspSegment)
        {
            return new DosProcess(this.FullPath, this.ImageName, pspSegment, this.EnvironmentSegment, this.CommandLineArguments)
            {
                DiskTransferAreaSegment = this.DiskTransferAreaSegment,
                DiskTransferAreaOffset = this.DiskTransferAreaOffset,
            };
        }
    }
}
