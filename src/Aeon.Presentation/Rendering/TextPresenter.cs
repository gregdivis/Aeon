using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Aeon.Emulator.Video;

namespace Aeon.Presentation.Rendering
{
    /// <summary>
    /// Renders text-mode graphics to a bitmap.
    /// </summary>
    internal sealed class TextPresenter : Presenter
    {
        private readonly int consoleWidth;
        private readonly int consoleHeight;
        private readonly int fontHeight;
        private readonly unsafe ushort*[] pages;
        private readonly byte[] font;
        private readonly unsafe byte* videoRam;

        /// <summary>
        /// Initializes a new instance of the TextPresenter class.
        /// </summary>
        /// <param name="dest">Pointer to destination bitmap.</param>
        /// <param name="videoMode">VideoMode instance describing the video mode.</param>
        public TextPresenter(IntPtr dest, VideoMode videoMode)
            : base(dest, videoMode)
        {
            unsafe
            {
                this.videoRam = (byte*)videoMode.VideoRam.ToPointer();
                byte* srcPtr = (byte*)videoMode.VideoRam.ToPointer();

                this.pages = new ushort*[8];
                for (int i = 0; i < this.pages.Length; i++)
                    this.pages[i] = (ushort*)(srcPtr + VideoMode.DisplayPageSize * i);
            }

            this.consoleWidth = videoMode.Width;
            this.consoleHeight = videoMode.Height;
            this.font = videoMode.Font;
            this.fontHeight = videoMode.FontHeight;
        }

        /// <summary>
        /// Updates the bitmap to match the current state of the video RAM.
        /// </summary>
        public override void Update()
        {
            var palette = this.VideoMode.Palette;
            byte[] internalPalette = this.VideoMode.InternalPalette;
            int displayPage = this.VideoMode.ActiveDisplayPage;

            unsafe
            {
                uint* destPtr = (uint*)this.Destination.ToPointer();

                byte* textPlane = this.videoRam + VideoMode.DisplayPageSize * displayPage;
                byte* attrPlane = this.videoRam + VideoMode.PlaneSize + VideoMode.DisplayPageSize * displayPage;

                for (int y = 0; y < consoleHeight; y++)
                {
                    for (int x = 0; x < consoleWidth; x++)
                    {
                        int srcOffset = y * consoleWidth + x;

                        uint* dest = destPtr + y * consoleWidth * 8 * fontHeight + x * 8;
                        DrawCharacter(dest, textPlane[srcOffset], palette[internalPalette[attrPlane[srcOffset] & 0x0F]], palette[internalPalette[attrPlane[srcOffset] >> 4]]);
                    }
                }
            }
        }

        /// <summary>
        /// Draws a single character to the bitmap.
        /// </summary>
        /// <param name="dest">Pointer in bitmap to top-left corner of the character.</param>
        /// <param name="index">Index of the character.</param>
        /// <param name="foregroundColor">Foreground color of the character.</param>
        /// <param name="backgroundColor">Background color of the character.</param>
        private unsafe void DrawCharacter(uint* dest, byte index, uint foregroundColor, uint backgroundColor)
        {
            if (Vector.IsHardwareAccelerated)
            {
                ReadOnlySpan<uint> indexes = stackalloc uint[] { 1 << 7, 1 << 6, 1 << 5, 1 << 4, 1 << 3, 1 << 2, 1 << 1, 1 << 0 };
                var indexVector = MemoryMarshal.Cast<uint, Vector<uint>>(indexes);
                var foregroundVector = new Vector<uint>(foregroundColor);
                var backgroundVector = new Vector<uint>(backgroundColor);

                for (int y = 0; y < this.fontHeight; y++)
                {
                    byte current = this.font[(index * fontHeight) + y];
                    var currentVector = new Vector<uint>(current);

                    int x = 0;

                    for (int i = 0; i < indexVector.Length; i++)
                    {
                        var maskResult = Vector.BitwiseAnd(currentVector, indexVector[i]);
                        var equalsResult = Vector.Equals(maskResult, indexVector[i]);
                        var result = Vector.ConditionalSelect(equalsResult, foregroundVector, backgroundVector);
                        for (int j = 0; j < Vector<uint>.Count; j++)
                            dest[x + j] = result[j];

                        x += Vector<uint>.Count;
                    }

                    dest += this.consoleWidth * 8;
                }
            }
            else
            {
                for (int y = 0; y < this.fontHeight; y++)
                {
                    byte current = this.font[(index * fontHeight) + y];

                    for (int x = 0; x < 8; x++)
                        dest[x] = (current & (1 << (7 - x))) != 0 ? foregroundColor : backgroundColor;

                    dest += this.consoleWidth * 8;
                }
            }
        }
    }
}
