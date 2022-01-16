using System;

namespace Aeon.Emulator.Video.Rendering
{
    /// <summary>
    /// Renders 8-bit mode X graphics to a bitmap.
    /// </summary>
    public sealed class GraphicsPresenterX : Presenter
    {
        private readonly unsafe byte*[] planes;

        /// <summary>
        /// Initializes a new instance of the GraphicsPresenterX class.
        /// </summary>
        /// <param name="dest">Pointer to destination bitmap.</param>
        /// <param name="videoMode">VideoMode instance describing the video mode.</param>
        public GraphicsPresenterX(VideoMode videoMode) : base(videoMode)
        {
            unsafe
            {
                this.planes = new byte*[4];
                byte* srcPtr = (byte*)videoMode.VideoRam.ToPointer();
                this.planes[0] = srcPtr + VideoMode.PlaneSize * 0;
                this.planes[1] = srcPtr + VideoMode.PlaneSize * 1;
                this.planes[2] = srcPtr + VideoMode.PlaneSize * 2;
                this.planes[3] = srcPtr + VideoMode.PlaneSize * 3;
            }
        }

        /// <summary>
        /// Updates the bitmap to match the current state of the video RAM.
        /// </summary>
        protected override void DrawFrame(IntPtr destination)
        {
            int width = this.VideoMode.Width;
            int height = this.VideoMode.Height;
            var palette = this.VideoMode.Palette;
            int startOffset = this.VideoMode.StartOffset;
            int stride = this.VideoMode.Stride;
            int lineCompare = this.VideoMode.LineCompare / 2;

            unsafe
            {
                uint* destPtr = (uint*)destination.ToPointer();

                int max = Math.Min(height, lineCompare + 1);

                for (int y = 0; y < max; y++)
                {
                    for (int x = 0; x < width; x++)
                        destPtr[(y * width) + x] = palette[planes[x % 4][(y * stride + (x / 4) + startOffset) & ushort.MaxValue]];
                }

                if (max < height)
                {
                    for (int y = max + 1; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                            destPtr[(y * width) + x] = palette[planes[x % 4][((y - max) * stride + (x / 4)) & ushort.MaxValue]];
                    }
                }
            }
        }
    }
}
