namespace Aeon.Emulator.CommandInterpreter;

public sealed class EchoCommand(string text) : CommandStatement
{
    public string Text { get; } = text;

    internal override CommandResult Run(CommandProcessor processor) => processor.RunCommand(this);
}
