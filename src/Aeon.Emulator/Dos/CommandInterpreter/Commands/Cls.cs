namespace Aeon.Emulator.CommandInterpreter.Commands
{
    /// <summary>
    /// The clear screen command.
    /// </summary>
    [Command("CLS")]
    public sealed class Cls : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Cls"/> class.
        /// </summary>
        public Cls()
        {
        }

        /// <summary>
        /// Attempts to run the command.
        /// </summary>
        /// <param name="vm">Current VirtualMachine instance.</param>
        /// <returns>Result of the command.</returns>
        public override CommandResult Run(VirtualMachine vm)
        {
            vm.Video?.TextConsole.Clear();
            return CommandResult.Continue;
        }
    }
}
