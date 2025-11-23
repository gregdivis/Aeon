namespace Aeon.Emulator.CommandInterpreter;

public sealed class InvalidCommand(string error) : CommandStatement
{
    public string Error { get; } = error;

    internal override CommandResult Run(CommandProcessor processor) => processor.RunCommand(this);
}
