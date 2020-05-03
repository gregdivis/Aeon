namespace Aeon.Emulator.Video.Modes
{
    /// <summary>
    /// A windowed VESA video mode.
    /// </summary>
    internal abstract class VesaWindowed : VideoMode
    {
        private const uint WindowSize = 65536;
        private const uint WindowGranularity = 65536;

        private unsafe readonly byte* videoRam;
        private uint windowOffset;
        private int firstPixel;
        private int firstScanLine;

        protected VesaWindowed(int width, int height, int bpp, bool planar, int fontHeight, VideoModeType modeType, VideoHandler video)
            : base(width, height, bpp, planar, fontHeight, VideoModeType.Graphics, video)
        {
            unsafe
            {
                this.videoRam = (byte*)video.VideoRam.ToPointer();
            }
        }

        /// <summary>
        /// Gets or sets the window position in granularity units.
        /// </summary>
        public uint WindowPosition
        {
            get => this.windowOffset / WindowGranularity;
            set => this.windowOffset = value * WindowGranularity;
        }
        /// <summary>
        /// Gets the number of bytes between rows of pixels.
        /// </summary>
        public override int Stride => this.Width;
        /// <summary>
        /// Gets the number of bytes from the beginning of video memory where the display data starts.
        /// </summary>
        public override int StartOffset => this.firstScanLine * this.Stride + this.firstPixel;

        /// <summary>
        /// Sets the position of the top-left corner of the display area.
        /// </summary>
        /// <param name="firstPixel">First pixel to display.</param>
        /// <param name="scanLine">First scan line to display.</param>
        public void SetDisplayStart(int firstPixel, int scanLine)
        {
            this.firstScanLine = scanLine;
            this.firstPixel = firstPixel;
        }

        internal override byte GetVramByte(uint offset)
        {
            unsafe
            {
                return this.videoRam[(offset + this.windowOffset) % VideoHandler.TotalVramBytes];
            }
        }
        internal override void SetVramByte(uint offset, byte value)
        {
            unsafe
            {
                this.videoRam[(offset + this.windowOffset) % VideoHandler.TotalVramBytes] = value;
            }
        }
        internal override ushort GetVramWord(uint offset)
        {
            unsafe
            {
                return *(ushort*)(this.videoRam + (offset + this.windowOffset) % VideoHandler.TotalVramBytes);
            }
        }
        internal override void SetVramWord(uint offset, ushort value)
        {
            unsafe
            {
                *(ushort*)(this.videoRam + (offset + this.windowOffset) % VideoHandler.TotalVramBytes) = value;
            }
        }
        internal override uint GetVramDWord(uint offset)
        {
            unsafe
            {
                return *(uint*)(this.videoRam + (offset + this.windowOffset) % VideoHandler.TotalVramBytes);
            }
        }
        internal override void SetVramDWord(uint offset, uint value)
        {
            unsafe
            {
                *(uint*)(this.videoRam + (offset + this.windowOffset) % VideoHandler.TotalVramBytes) = value;
            }
        }
        internal override void WriteCharacter(int x, int y, int index, byte foreground, byte background)
        {
        }
    }
}
