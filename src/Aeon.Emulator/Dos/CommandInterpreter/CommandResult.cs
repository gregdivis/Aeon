namespace Aeon.Emulator.CommandInterpreter;

/// <summary>
/// Specifies what a command interpreter should do after running a command.
/// </summary>
public enum CommandResult
{
    /// <summary>
    /// The command interpter should continue running.
    /// </summary>
    Continue,
    /// <summary>
    /// The command interpreter should suspend running until the launched process completes.
    /// </summary>
    Launch,
    /// <summary>
    /// The command interpreter should exit.
    /// </summary>
    Exit
}
