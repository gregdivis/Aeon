namespace Aeon.Emulator.CommandInterpreter
{
    public abstract class CommandStatement
    {
        protected CommandStatement()
        {
        }

        public bool NoEcho { get; internal set; }
        public string RawStatement { get; internal set; }

        internal abstract CommandResult Run(CommandProcessor processor);

        public override string ToString() => this.RawStatement;
    }
}
