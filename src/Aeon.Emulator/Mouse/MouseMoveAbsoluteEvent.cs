namespace Aeon.Emulator;

/// <summary>
/// Contains information about a mouse movement in absolute coordinates.
/// </summary>
public sealed class MouseMoveAbsoluteEvent : MouseEvent
{
    private readonly uint x;
    private readonly uint y;

    /// <summary>
    /// Initializes a new instance of the <see cref="MouseMoveAbsoluteEvent"/> class.
    /// </summary>
    /// <param name="x">New X position of the mouse.</param>
    /// <param name="y">New Y position of the mouse.</param>
    public MouseMoveAbsoluteEvent(int x, int y)
    {
        if (x < 0)
            this.x = 0;
        else
            this.x = (uint)x;

        if (y < 0)
            this.y = 0;
        else
            this.y = (uint)y;
    }

    /// <summary>
    /// Gets the X position of the mouse.
    /// </summary>
    public int X => (int)x;
    /// <summary>
    /// Gets the Y position of the mouse.
    /// </summary>
    public int Y => (int)y;

    /// <summary>
    /// Gets a string representation of the mouse position.
    /// </summary>
    /// <returns>String representation of the mouse position.</returns>
    public override string ToString() => $"Mouse position: ({x}, {y})";

    /// <summary>
    /// Raises the event on the emulated mouse device.
    /// </summary>
    /// <param name="mouse">Emulated mouse device instance.</param>
    internal override void RaiseEvent(Mouse.MouseHandler mouse) => mouse.MouseMoveAbsolute(x, y);
}
