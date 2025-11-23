namespace Aeon.Emulator.RuntimeExceptions;

/// <summary>
/// Represents an emulated integer divide-by-zero exception.
/// </summary>
public sealed class EmulatedDivideByZeroException : EmulatedException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmulatedDivideByZeroException"/> class.
    /// </summary>
    public EmulatedDivideByZeroException()
        : base(0, "Cannot divide by zero.")
    {
    }
}
