namespace Aeon.Emulator.CommandInterpreter;

public sealed class LaunchCommand(string target, string arguments) : CommandStatement
{
    public string Target { get; } = target;
    public string Arguments { get; } = arguments;

    internal override CommandResult Run(CommandProcessor processor) => processor.RunCommand(this);
}
