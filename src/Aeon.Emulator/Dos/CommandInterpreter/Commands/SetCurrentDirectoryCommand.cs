namespace Aeon.Emulator.CommandInterpreter;

public sealed class SetCurrentDirectoryCommand(string path) : CommandStatement
{
    public string Path { get; } = path;

    internal override CommandResult Run(CommandProcessor processor) => processor.RunCommand(this);
}
