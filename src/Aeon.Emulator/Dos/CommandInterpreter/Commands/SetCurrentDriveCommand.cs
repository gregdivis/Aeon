using Aeon.Emulator.Dos.VirtualFileSystem;

namespace Aeon.Emulator.CommandInterpreter;

public sealed class SetCurrentDriveCommand(DriveLetter driveLetter) : CommandStatement
{
    public DriveLetter Drive { get; } = driveLetter;

    internal override CommandResult Run(CommandProcessor processor) => processor.RunCommand(this);
}
