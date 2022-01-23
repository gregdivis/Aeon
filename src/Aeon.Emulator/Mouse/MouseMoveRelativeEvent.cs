namespace Aeon.Emulator
{
    /// <summary>
    /// Contains information about a mouse movement in relative units.
    /// </summary>
    public sealed class MouseMoveRelativeEvent : MouseEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MouseMoveRelativeEvent"/> class.
        /// </summary>
        /// <param name="deltaX">Horizontal movement in screen pixels.</param>
        /// <param name="deltaY">Vertical movement in screen pixels.</param>
        public MouseMoveRelativeEvent(int deltaX, int deltaY)
        {
            this.DeltaX = deltaX;
            this.DeltaY = deltaY;
        }

        /// <summary>
        /// Gets the horizontal movement amount.
        /// </summary>
        public int DeltaX { get; }
        /// <summary>
        /// Gets the vertical movement amount.
        /// </summary>
        public int DeltaY { get; }

        /// <summary>
        /// Gets a string representation of the mouse position change.
        /// </summary>
        /// <returns>String representation of the mouse position change.</returns>
        public override string ToString() => $"Mouse movement: ({this.DeltaX}, {this.DeltaY})";

        internal override void RaiseEvent(Mouse.MouseHandler mouse) => mouse.MouseMoveRelative(this.DeltaX, this.DeltaY);
    }
}
