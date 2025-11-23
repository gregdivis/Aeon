namespace Aeon.Emulator.CommandInterpreter;

public sealed class PrintCurrentDirectoryCommand : CommandStatement
{
    public PrintCurrentDirectoryCommand()
    {
    }

    internal override CommandResult Run(CommandProcessor processor) => processor.RunCommand(this);
}
