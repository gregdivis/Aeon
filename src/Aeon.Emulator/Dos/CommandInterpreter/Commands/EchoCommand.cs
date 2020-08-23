namespace Aeon.Emulator.CommandInterpreter
{
    public sealed class EchoCommand : CommandStatement
    {
        public EchoCommand(string text)
        {
            this.Text = text;
        }

        public string Text { get; }

        internal override CommandResult Run(CommandProcessor processor) => processor.RunCommand(this);
    }
}
