namespace Aeon.Emulator
{
    /// <summary>
    /// Contains information about a mouse button release.
    /// </summary>
    public sealed class MouseButtonUpEvent : MouseEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MouseButtonUpEvent"/> class.
        /// </summary>
        /// <param name="button">Button which was released.</param>
        public MouseButtonUpEvent(MouseButtons button) => this.Button = button;

        /// <summary>
        /// Gets the button which was released.
        /// </summary>
        public MouseButtons Button { get; }

        /// <summary>
        /// Gets a string representation of the mouse button released.
        /// </summary>
        /// <returns>String representation of the mouse button released.</returns>
        public override string ToString() => "Mouse button released: " + this.Button;

        internal override void RaiseEvent(Mouse.MouseHandler mouse) => mouse.MouseButtonUp(this.Button);
    }
}
