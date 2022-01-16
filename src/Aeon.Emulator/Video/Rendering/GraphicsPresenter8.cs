using System;

namespace Aeon.Emulator.Video.Rendering
{
    /// <summary>
    /// Renders 8-bit graphics to a bitmap.
    /// </summary>
    public sealed class GraphicsPresenter8 : Presenter
    {
        /// <summary>
        /// Initializes a new instance of the GraphicsPresenter8 class.
        /// </summary>
        /// <param name="videoMode">VideoMode instance describing the video mode.</param>
        public GraphicsPresenter8(VideoMode videoMode) : base(videoMode)
        {
        }

        /// <summary>
        /// Updates the bitmap to match the current state of the video RAM.
        /// </summary>
        protected override void DrawFrame(IntPtr destination)
        {
            uint totalPixels = (uint)this.VideoMode.Width * (uint)this.VideoMode.Height;
            var palette = this.VideoMode.Palette;

            unsafe
            {
                byte* srcPtr = (byte*)this.VideoMode.VideoRam.ToPointer() + (uint)this.VideoMode.StartOffset;
                uint* destPtr = (uint*)destination.ToPointer();

                for (int i = 0; i < totalPixels; i++)
                    destPtr[i] = palette[srcPtr[i]];
            }
        }
    }
}
