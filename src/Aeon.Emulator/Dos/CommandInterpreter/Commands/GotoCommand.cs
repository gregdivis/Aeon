namespace Aeon.Emulator.CommandInterpreter;

public sealed class GotoCommand(string label) : CommandStatement
{
    public string Label { get; } = label;

    internal override CommandResult Run(CommandProcessor processor) => processor.RunCommand(this);
}
