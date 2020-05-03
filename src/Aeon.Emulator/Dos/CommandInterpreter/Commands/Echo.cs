namespace Aeon.Emulator.CommandInterpreter.Commands
{
    /// <summary>
    /// The echo command.
    /// </summary>
    [Command("ECHO")]
    public sealed class Echo : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Echo"/> class.
        /// </summary>
        public Echo()
        {
        }

        /// <summary>
        /// Gets the text to display.
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// Attempts to run the command.
        /// </summary>
        /// <param name="vm">Current VirtualMachine instance.</param>
        /// <returns>Result of the command.</returns>
        public override CommandResult Run(VirtualMachine vm)
        {
            vm.Console.WriteLine(this.Text);
            return CommandResult.Continue;
        }

        /// <summary>
        /// Attempts to parse the command's arguments.
        /// </summary>
        /// <param name="arguments">Arguments to parse.</param>
        /// <returns>Value indicating whether the parsing was successful.</returns>
        protected override bool ParseArguments(string arguments)
        {
            this.Text = arguments;
            return true;
        }
    }
}
