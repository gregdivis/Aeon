namespace Aeon.Emulator.CommandInterpreter
{
    public abstract class CommandStatement
    {
        protected CommandStatement()
        {
        }

        public bool NoEcho { get; internal init; }

        internal abstract CommandResult Run(CommandProcessor processor);
    }
}
