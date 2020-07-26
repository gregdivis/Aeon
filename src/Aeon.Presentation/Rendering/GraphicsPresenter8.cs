using System;
using Aeon.Emulator.Video;

namespace Aeon.Presentation.Rendering
{
    /// <summary>
    /// Renders 8-bit graphics to a bitmap.
    /// </summary>
    internal sealed class GraphicsPresenter8 : Presenter
    {
        /// <summary>
        /// Initializes a new instance of the GraphicsPresenter8 class.
        /// </summary>
        /// <param name="dest">Pointer to destination bitmap.</param>
        /// <param name="videoMode">VideoMode instance describing the video mode.</param>
        public GraphicsPresenter8(IntPtr dest, VideoMode videoMode)
            : base(dest, videoMode)
        {
        }

        /// <summary>
        /// Updates the bitmap to match the current state of the video RAM.
        /// </summary>
        public override void Update()
        {
            uint totalPixels = (uint)this.VideoMode.Width * (uint)this.VideoMode.Height;
            var palette = this.VideoMode.Palette;

            unsafe
            {
                byte* srcPtr = (byte*)this.VideoMode.VideoRam.ToPointer() + (uint)this.VideoMode.StartOffset;
                uint* destPtr = (uint*)this.Destination.ToPointer();

                for (int i = 0; i < totalPixels; i++)
                    destPtr[i] = palette[srcPtr[i]];
            }
        }
    }
}
