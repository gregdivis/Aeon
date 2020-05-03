namespace Aeon.Emulator.CommandInterpreter.Commands
{
    /// <summary>
    /// The goto command.
    /// </summary>
    [Command("GOTO")]
    public sealed class Goto : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Goto"/> class.
        /// </summary>
        public Goto()
        {
        }

        /// <summary>
        /// Gets the name of the target label.
        /// </summary>
        public string TargetLabel { get; private set; }

        /// <summary>
        /// Attempts to parse the command's arguments.
        /// </summary>
        /// <param name="arguments">Arguments to parse.</param>
        /// <returns>Value indicating whether the parsing was successful.</returns>
        protected override bool ParseArguments(string arguments)
        {
            if (string.IsNullOrEmpty(arguments))
                return false;

            this.TargetLabel = arguments;
            return true;
        }
    }
}
