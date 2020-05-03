namespace Aeon.Emulator.CommandInterpreter.Commands
{
    /// <summary>
    /// The exit command.
    /// </summary>
    [Command("EXIT")]
    public sealed class Exit : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Exit"/> command.
        /// </summary>
        public Exit()
        {
        }

        /// <summary>
        /// Attempts to run the command.
        /// </summary>
        /// <param name="vm">Current VirtualMachine instance.</param>
        /// <returns>Result of the command.</returns>
        public override CommandResult Run(VirtualMachine vm) => CommandResult.Exit;
    }
}
