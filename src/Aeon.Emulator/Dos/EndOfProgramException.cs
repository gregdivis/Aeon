namespace Aeon.Emulator;

/// <summary>
/// Exception thrown when the emulated process should terminate.
/// </summary>
public sealed class EndOfProgramException : Exception
{
    /// <summary>
    /// Initializes a new instance of the EndOfProgramException class.
    /// </summary>
    public EndOfProgramException()
    {
    }
}
