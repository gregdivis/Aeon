namespace Aeon.Emulator;

/// <summary>
/// Contains information about a mouse button press.
/// </summary>
public sealed class MouseButtonDownEvent : MouseEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MouseButtonDownEvent"/> class.
    /// </summary>
    /// <param name="button">Button which was pressed.</param>
    public MouseButtonDownEvent(MouseButtons button) => this.Button = button;

    /// <summary>
    /// Gets the button which was pressed.
    /// </summary>
    public MouseButtons Button { get; }

    /// <summary>
    /// Gets a string representation of the mouse button pressed.
    /// </summary>
    /// <returns>String representation of the mouse button pressed.</returns>
    public override string ToString() => $"Mouse button pressed: {this.Button}";

    internal override void RaiseEvent(Mouse.MouseHandler mouse) => mouse.MouseButtonDown(this.Button);
}
