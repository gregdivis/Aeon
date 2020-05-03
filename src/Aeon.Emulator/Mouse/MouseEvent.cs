using System;

namespace Aeon.Emulator
{
    /// <summary>
    /// Describes a mouse input event.
    /// </summary>
    public abstract class MouseEvent
    {
        internal MouseEvent()
        {
        }

        /// <summary>
        /// Raises the event on the emulated mouse device.
        /// </summary>
        /// <param name="mouse">Emulated mouse device instance.</param>
        internal abstract void RaiseEvent(Mouse.MouseHandler mouse);
    }

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
        public override string ToString() => "Mouse button pressed: " + this.Button;

        internal override void RaiseEvent(Mouse.MouseHandler mouse) => mouse.MouseButtonDown(this.Button);
    }

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

    /// <summary>
    /// Provides information about a mouse movement.
    /// </summary>
    public sealed class MouseMoveEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MouseMoveEventArgs"/> class.
        /// </summary>
        /// <param name="x">X position of the mouse.</param>
        /// <param name="y">Y position of the mouse.</param>
        public MouseMoveEventArgs(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Gets the X position of the mouse.
        /// </summary>
        public int X { get; }
        /// <summary>
        /// Gets the Y position of the mouse.
        /// </summary>
        public int Y { get; }
    }

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
}
