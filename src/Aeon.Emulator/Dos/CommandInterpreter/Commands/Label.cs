namespace Aeon.Emulator.CommandInterpreter.Commands
{
    /// <summary>
    /// A batch file label.
    /// </summary>
    [Command(":")]
    public sealed class Label : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Label"/> class.
        /// </summary>
        public Label()
        {
        }

        /// <summary>
        /// Gets the name of the label.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Attempts to parse the command's arguments.
        /// </summary>
        /// <param name="arguments">Arguments to parse.</param>
        /// <returns>Value indicating whether the parsing was successful.</returns>
        protected override bool ParseArguments(string arguments)
        {
            if (string.IsNullOrEmpty(arguments))
                return false;

            this.Name = arguments;
            return true;
        }
    }
}
