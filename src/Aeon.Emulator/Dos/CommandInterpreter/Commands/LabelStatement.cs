namespace Aeon.Emulator.CommandInterpreter;

public sealed class LabelStatement(string name) : CommandStatement
{
    public string Name { get; } = name;

    internal override CommandResult Run(CommandProcessor processor) => CommandResult.Continue;
}
