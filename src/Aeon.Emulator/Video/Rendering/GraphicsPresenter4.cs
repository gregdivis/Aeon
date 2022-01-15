using System;
using Aeon.Emulator.Video;

namespace Aeon.Presentation.Rendering
{
    /// <summary>
    /// Renders 4-bit graphics to a bitmap.
    /// </summary>
    internal sealed class GraphicsPresenter4 : Presenter
    {
        private readonly unsafe byte*[] planes;

        /// <summary>
        /// Initializes a new instance of the GraphicsPresenter4 class.
        /// </summary>
        /// <param name="dest">Pointer to destination bitmap.</param>
        /// <param name="videoMode">VideoMode instance describing the video mode.</param>
        public GraphicsPresenter4(IntPtr dest, VideoMode videoMode)
            : base(dest, videoMode)
        {
            unsafe
            {
                byte* srcPtr = (byte*)videoMode.VideoRam.ToPointer();
                this.planes = new byte*[4];
                this.planes[0] = srcPtr + VideoMode.PlaneSize * 0;
                this.planes[1] = srcPtr + VideoMode.PlaneSize * 1;
                this.planes[2] = srcPtr + VideoMode.PlaneSize * 2;
                this.planes[3] = srcPtr + VideoMode.PlaneSize * 3;
            }
        }

        /// <summary>
        /// Updates the bitmap to match the current state of the video RAM.
        /// </summary>
        public override void Update()
        {
            int width = this.VideoMode.Width;
            int height = Math.Min(this.VideoMode.Height, this.VideoMode.LineCompare + 1);
            var palette = this.VideoMode.Palette;
            byte[] paletteMap = this.VideoMode.InternalPalette;
            int stride = this.VideoMode.Stride;
            int horizontalPan = this.VideoMode.HorizontalPanning;
            int startOffset = this.VideoMode.StartOffset;

            int safeWidth = Math.Min(stride, width / 8);
            bool bitPan = (horizontalPan % 8) != 0;

            unsafe
            {
                uint* destPtr = (uint*)this.Destination.ToPointer();
                int destStart = 0;

                for (int split = 0; split < 2; split++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        int srcPos = (stride * y + startOffset + horizontalPan / 8) & 0xFFFF;
                        int destPos = width * y + destStart;

                        byte index;
                        for (int i = horizontalPan % 8; i < 8; i++)
                        {
                            index = 0;
                            if ((planes[0][srcPos] & (0x80 >> i)) != 0)
                                index |= 1;
                            if ((planes[1][srcPos] & (0x80 >> i)) != 0)
                                index |= 2;
                            if ((planes[2][srcPos] & (0x80 >> i)) != 0)
                                index |= 4;
                            if ((planes[3][srcPos] & (0x80 >> i)) != 0)
                                index |= 8;

                            destPtr[destPos] = palette[paletteMap[index]];
                            destPos++;
                        }
                        srcPos++;

                        // This loop isn't terribly elegant, but it gets the job done and it's reasonably fast.
                        // Each iteration draws 8 horizontal pixels by shifting bits from the appropriate plane into
                        // a palette index.
                        for (int xb = 1; xb < safeWidth; xb++)
                        {
                            uint p1 = planes[0][srcPos];
                            uint p2 = planes[1][srcPos];
                            uint p3 = planes[2][srcPos];
                            uint p4 = planes[3][srcPos];

                            uint palIndex = (p1 & 1u) | ((p2 & 1u) << 1) | ((p3 & 1u) << 2) | ((p4 & 1u) << 3);
                            destPtr[destPos + 7] = palette[paletteMap[palIndex]];

                            palIndex = ((p1 & 2u) >> 1) | (p2 & 2u) | ((p3 & 2u) << 1) | ((p4 & 2u) << 2);
                            destPtr[destPos + 6] = palette[paletteMap[palIndex]];

                            palIndex = ((p1 & 4u) >> 2) | ((p2 & 4u) >> 1) | (p3 & 4u) | ((p4 & 4u) << 1);
                            destPtr[destPos + 5] = palette[paletteMap[palIndex]];

                            palIndex = ((p1 & 8u) >> 3) | ((p2 & 8u) >> 2) | ((p3 & 8u) >> 1) | (p4 & 8u);
                            destPtr[destPos + 4] = palette[paletteMap[palIndex]];

                            palIndex = ((p1 & 16u) >> 4) | ((p2 & 16u) >> 3) | ((p3 & 16u) >> 2) | ((p4 & 16u) >> 1);
                            destPtr[destPos + 3] = palette[paletteMap[palIndex]];

                            palIndex = ((p1 & 32u) >> 5) | ((p2 & 32u) >> 4) | ((p3 & 32u) >> 3) | ((p4 & 32u) >> 2);
                            destPtr[destPos + 2] = palette[paletteMap[palIndex]];

                            palIndex = ((p1 & 64u) >> 6) | ((p2 & 64u) >> 5) | ((p3 & 64u) >> 4) | ((p4 & 64u) >> 3);
                            destPtr[destPos + 1] = palette[paletteMap[palIndex]];

                            palIndex = ((p1 & 128u) >> 7) | ((p2 & 128u) >> 6) | ((p3 & 128u) >> 5) | ((p4 & 128u) >> 4);
                            destPtr[destPos] = palette[paletteMap[palIndex]];

                            destPos += 8;
                            srcPos++;
                        }

                        if (bitPan)
                        {
                            for (int i = 7 - (horizontalPan % 8); i < 8; i++)
                            {
                                index = 0;
                                if ((planes[0][srcPos] & (0x80 >> i)) != 0)
                                    index |= 1;
                                if ((planes[1][srcPos] & (0x80 >> i)) != 0)
                                    index |= 2;
                                if ((planes[2][srcPos] & (0x80 >> i)) != 0)
                                    index |= 4;
                                if ((planes[3][srcPos] & (0x80 >> i)) != 0)
                                    index |= 8;

                                destPtr[destPos] = palette[paletteMap[index]];
                                destPos++;
                            }
                        }
                    }

                    if (height < this.VideoMode.Height)
                    {
                        startOffset = 0;
                        height = this.VideoMode.Height - this.VideoMode.LineCompare - 1;
                        destStart = this.VideoMode.LineCompare * width;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }
}
