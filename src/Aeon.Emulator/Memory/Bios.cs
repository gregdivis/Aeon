namespace Aeon.Emulator
{
    /// <summary>
    /// Provides access to emulated memory mapped BIOS values.
    /// </summary>
    internal sealed class Bios
    {
        private readonly PhysicalMemory memory;

        public Bios(PhysicalMemory memory)
        {
            this.memory = memory;
            this.VideoMode = VideoMode10.ColorText80x25x4;
            this.ScreenRows = 24;
            this.ScreenColumns = 80;
            this.CharacterPointHeight = 16;
            this.CrtControllerBaseAddress = 0x03D4;
            this.memory.Reserve(0x40, 256);
        }

        /// <summary>
        /// Gets or sets the BIOS video mode.
        /// </summary>
        public VideoMode10 VideoMode
        {
            get => (VideoMode10)memory.GetByte(0x0040, 0x0049);
            set => memory.SetByte(0x0040, 0x0049, (byte)value);
        }
        /// <summary>
        /// Gets or sets the BIOS screen column count.
        /// </summary>
        public byte ScreenColumns
        {
            get => memory.GetByte(0x0040, 0x004A);
            set => memory.SetByte(0x0040, 0x004A, value);
        }
        /// <summary>
        /// Gets or sets the current value of the real time clock.
        /// </summary>
        public uint RealTimeClock
        {
            get => memory.GetUInt32(0x0040, 0x006C);
            set => memory.SetUInt32(0x0040, 0x006C, value);
        }
        /// <summary>
        /// Gets or sets the BIOS screen row count.
        /// </summary>
        public byte ScreenRows
        {
            get => memory.GetByte(0x0040, 0x0084);
            set => memory.SetByte(0x0040, 0x0084, value);
        }
        /// <summary>
        /// Gets or sets the character point height.
        /// </summary>
        public ushort CharacterPointHeight
        {
            get => memory.GetUInt16(0x0040, 0x0085);
            set => memory.SetUInt16(0x0040, 0x0085, value);
        }
        /// <summary>
        /// Gets or sets the BIOS video mode options.
        /// </summary>
        public byte VideoModeOptions
        {
            get => memory.GetByte(0x0040, 0x0087);
            set => memory.SetByte(0x0040, 0x0087, value);
        }
        /// <summary>
        /// Gets or sets the EGA feature switch values.
        /// </summary>
        public byte FeatureSwitches
        {
            get => memory.GetByte(0x0040, 0x0088);
            set => memory.SetByte(0x0040, 0x0088, value);
        }
        /// <summary>
        /// Gets or sets the video display data value.
        /// </summary>
        public byte VideoDisplayData
        {
            get => memory.GetByte(0x0040, 0x0089);
            set => memory.SetByte(0x0040, 0x0089, value);
        }
        /// <summary>
        /// Gets or sets the CRT controller base address.
        /// </summary>
        public ushort CrtControllerBaseAddress
        {
            get => memory.GetUInt16(0x0040, 0x0063);
            set => memory.SetUInt16(0x0040, 0x0063, value);
        }
        /// <summary>
        /// Gets or sets the value of the disk motor timer.
        /// </summary>
        public byte DiskMotorTimer
        {
            get => this.memory.GetByte(0x0040, 0x0040);
            set => this.memory.SetByte(0x0040, 0x0040, value);
        }
    }

    /// <summary>
    /// Specifies one of the int 10h video modes.
    /// </summary>
    internal enum VideoMode10
    {
        /// <summary>
        /// Monochrome 40x25 text mode.
        /// </summary>
        Text40x25x1 = 0x00,
        /// <summary>
        /// Color 40x25 text mode (4-bit).
        /// </summary>
        ColorText40x25x4 = 0x01,
        /// <summary>
        /// Monochrome 80x25 text mode (4-bit).
        /// </summary>
        MonochromeText80x25x4 = 0x02,
        /// <summary>
        /// Color 80x25 text mode (4-bit).
        /// </summary>
        ColorText80x25x4 = 0x03,
        /// <summary>
        /// Color 320x200 graphics mode (2-bit).
        /// </summary>
        ColorGraphics320x200x2A = 0x04,
        /// <summary>
        /// Color 320x200 graphics mode (2-bit).
        /// </summary>
        ColorGraphics320x200x2B = 0x05,
        /// <summary>
        /// Monochrome 640x200 graphics mode (1-bit).
        /// </summary>
        Graphics640x200x1 = 0x06,
        /// <summary>
        /// Monochrome 80x25 text mode (1-bit).
        /// </summary>
        Text80x25x1 = 0x07,
        /// <summary>
        /// Color 320x200 graphics mode (4-bit).
        /// </summary>
        ColorGraphics320x200x4 = 0x0D,
        /// <summary>
        /// Color 640x200 graphics mode (4-bit).
        /// </summary>
        ColorGraphics640x200x4 = 0x0E,
        /// <summary>
        /// Monochrome 640x350 graphics mode (1-bit).
        /// </summary>
        Graphics640x350x1 = 0x0F,
        /// <summary>
        /// Color 640x350 graphics mode (4-bit).
        /// </summary>
        ColorGraphics640x350x4 = 0x10,
        /// <summary>
        /// Monochrome 640x480 graphics mode (1-bit).
        /// </summary>
        Graphics640x480x1 = 0x11,
        /// <summary>
        /// Color 640x480 graphics mode (4-bit).
        /// </summary>
        Graphics640x480x4 = 0x12,
        /// <summary>
        /// Color 320x200 graphics mode (8-bit).
        /// </summary>
        Graphics320x200x8 = 0x13
    }
}
