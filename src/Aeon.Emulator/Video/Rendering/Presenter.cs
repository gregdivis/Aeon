using System;
using Aeon.Emulator.Video;

namespace Aeon.Presentation.Rendering
{
    /// <summary>
    /// Renders emulated video RAM data to a bitmap.
    /// </summary>
    internal abstract class Presenter
    {
        /// <summary>
        /// Initializes a new instance of the Presenter class.
        /// </summary>
        /// <param name="dest">Pointer to destination bitmap.</param>
        /// <param name="videoMode">VideoMode instance describing the video mode.</param>
        protected Presenter(IntPtr dest, VideoMode videoMode)
        {
            this.Destination = dest;
            this.VideoMode = videoMode;
        }

        /// <summary>
        /// Gets information about the video mode.
        /// </summary>
        protected VideoMode VideoMode { get; }
        /// <summary>
        /// Gets a pointer to the destination bitmap.
        /// </summary>
        protected IntPtr Destination { get; }

        /// <summary>
        /// Updates the bitmap to match the current state of the video RAM.
        /// </summary>
        public abstract void Update();
    }
}
