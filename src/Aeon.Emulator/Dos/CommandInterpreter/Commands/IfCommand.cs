namespace Aeon.Emulator.CommandInterpreter;

public abstract class IfCommand(bool not, CommandStatement command) : CommandStatement
{
    public bool Not { get; } = not;
    public CommandStatement Command { get; } = command;
}

public sealed class IfErrorLevelCommand(bool not, int errorLevel, CommandStatement command) : IfCommand(not, command)
{
    public int ErrorLevel { get; } = errorLevel;

    internal override CommandResult Run(CommandProcessor processor) => processor.RunCommand(this);
}

public sealed class IfFileExistsCommand(bool not, string fileName, CommandStatement command) : IfCommand(not, command)
{
    public string FileName { get; } = fileName;

    internal override CommandResult Run(CommandProcessor processor) => processor.RunCommand(this);
}

public sealed class IfEqualsCommand(bool not, string value1, string value2, CommandStatement command) : IfCommand(not, command)
{
    public string Value1 { get; } = value1;
    public string Value2 { get; } = value2;

    internal override CommandResult Run(CommandProcessor processor) => processor.RunCommand(this);
}
