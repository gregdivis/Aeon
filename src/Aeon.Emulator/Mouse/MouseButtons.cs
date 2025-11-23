namespace Aeon.Emulator;

/// <summary>
/// Represents a mouse button.
/// </summary>
[Flags]
public enum MouseButtons
{
    /// <summary>
    /// No mouse button.
    /// </summary>
    None = 0,
    /// <summary>
    /// The left mouse button.
    /// </summary>
    Left = 1,
    /// <summary>
    /// The right mouse button.
    /// </summary>
    Right = 2,
    /// <summary>
    /// The middle mouse button.
    /// </summary>
    Middle = 4
}
