using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Aeon.Emulator.Video;

namespace Aeon.Presentation.Rendering
{
    /// <summary>
    /// Renders text-mode graphics to a bitmap.
    /// </summary>
    internal sealed class TextPresenter : Presenter
    {
        private readonly uint consoleWidth;
        private readonly uint consoleHeight;
        private readonly uint fontHeight;
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

            this.consoleWidth = (uint)videoMode.Width;
            this.consoleHeight = (uint)videoMode.Height;
            this.font = videoMode.Font;
            this.fontHeight = (uint)videoMode.FontHeight;
        }

        /// <summary>
        /// Updates the bitmap to match the current state of the video RAM.
        /// </summary>
        public override void Update()
        {
            unsafe
            {
                var palette = this.VideoMode.Palette;
                byte* internalPalette = stackalloc byte[16];
                this.VideoMode.InternalPalette.CopyTo(new Span<byte>(internalPalette, 16));
                uint displayPage = (uint)this.VideoMode.ActiveDisplayPage;

                uint* destPtr = (uint*)this.Destination.ToPointer();

                byte* textPlane = this.videoRam + VideoMode.DisplayPageSize * displayPage;
                byte* attrPlane = this.videoRam + VideoMode.PlaneSize + VideoMode.DisplayPageSize * displayPage;

                for (uint y = 0; y < this.consoleHeight; y++)
                {
                    for (uint x = 0; x < this.consoleWidth; x++)
                    {
                        uint srcOffset = y * this.consoleWidth + x;

                        uint* dest = destPtr + y * this.consoleWidth * 8 * this.fontHeight + x * 8;
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
