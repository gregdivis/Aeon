using Aeon.Emulator.Dos.VirtualFileSystem;

namespace Aeon.Emulator.CommandInterpreter
{
    public sealed class SetCurrentDriveCommand : CommandStatement
    {
        public SetCurrentDriveCommand(DriveLetter driveLetter)
        {
            this.Drive = driveLetter;
        }

        public DriveLetter Drive { get; }

        internal override CommandResult Run(CommandProcessor processor) => processor.RunCommand(this);
    }
}
