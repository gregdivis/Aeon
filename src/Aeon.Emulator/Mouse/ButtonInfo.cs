namespace Aeon.Emulator.Mouse;

/// <summary>
/// Stores information about a button press or release.
/// </summary>
internal struct ButtonInfo
{
    /// <summary>
    /// Number of times the button was pressed or released.
    /// </summary>
    public uint Count;
    /// <summary>
    /// X-coordinate of the cursor at the most recent event.
    /// </summary>
    public int X;
    /// <summary>
    /// Y-coordinate of the cursor at the most recent event.
    /// </summary>
    public int Y;
}
