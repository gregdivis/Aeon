using System;

namespace Aeon.Emulator
{
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
}
