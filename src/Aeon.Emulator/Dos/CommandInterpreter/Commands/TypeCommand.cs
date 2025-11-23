namespace Aeon.Emulator.CommandInterpreter;

public sealed class TypeCommand(string fileName) : CommandStatement
{
    public string FileName { get; } = fileName;

    internal override CommandResult Run(CommandProcessor processor) => processor.RunCommand(this);
}
