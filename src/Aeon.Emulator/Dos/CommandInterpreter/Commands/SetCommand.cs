namespace Aeon.Emulator.CommandInterpreter;

public sealed class SetCommand(string variable, string value) : CommandStatement
{
    public string Variable { get; } = variable;
    public string Value { get; } = value;

    internal override CommandResult Run(CommandProcessor processor) => processor.RunCommand(this);
}
