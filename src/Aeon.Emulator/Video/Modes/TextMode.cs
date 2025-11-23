namespace Aeon.Emulator.Video.Modes;

/// <summary>
/// Implements functionality for text video modes.
/// </summary>
internal sealed class TextMode : VideoMode
{
    private const uint BaseAddress = 0x18000;

    private readonly UnsafeBuffer<nint> planesBuffer = new(4);
    private readonly unsafe byte* videoRam;
    private readonly unsafe byte** planes;
    private readonly Graphics graphics;
    private readonly Sequencer sequencer;

    public TextMode(int width, int height, int fontHeight, VideoHandler video)
        : base(width, height, 4, false, fontHeight, VideoModeType.Text, video)
    {
        unsafe
        {
            this.videoRam = (byte*)video.VideoRam.ToPointer();
            byte* vram = this.videoRam;
            this.planes = (byte**)this.planesBuffer.ToPointer();

            this.planes[0] = vram + PlaneSize * 0;
            this.planes[1] = vram + PlaneSize * 1;
            this.planes[2] = vram + PlaneSize * 2;
            this.planes[3] = vram + PlaneSize * 3;
        }

        this.graphics = video.Graphics;
        this.sequencer = video.Sequencer;
    }

    /// <summary>
    /// Gets a value indicating whether the display mode has a cursor.
    /// </summary>
    internal override bool HasCursor => true;

    /// <summary>
    /// Gets a value indicating whether odd-even write addressing is enabled.
    /// </summary>
    private bool IsOddEvenWriteEnabled => (this.sequencer.SequencerMemoryMode & SequencerMemoryMode.OddEvenWriteAddressingDisabled) == 0;
    /// <summary>
    /// Gets a value indicating whether odd-even read addressing is enabled.
    /// </summary>
    private bool IsOddEvenReadEnabled => (this.graphics.GraphicsMode & 0x10) != 0;

    internal override byte GetVramByte(uint offset)
    {
        if (offset - BaseAddress >= VideoHandler.TotalVramBytes)
            return 0;

        unsafe
        {
            uint address = offset - BaseAddress;

            if (this.IsOddEvenReadEnabled)
            {
                return this.planes[address & 1][address >> 1];
            }
            else
            {
                var map = this.graphics.ReadMapSelect & 0x3;
                if (map == 0 || map == 1)
                    return this.planes[map][address];
                else if (map == 3)
                    return this.Font[address % 4096];
                else
                    return 0;
            }
        }
    }
    internal override void SetVramByte(uint offset, byte value)
    {
        if (offset - BaseAddress >= VideoHandler.TotalVramBytes)
            return;

        unsafe
        {
            uint address = offset - BaseAddress;

            if (this.IsOddEvenWriteEnabled)
            {
                this.planes[address & 1][address >> 1] = value;
            }
            else
            {
                uint mapMask = this.sequencer.MapMask.Packed;
                if ((mapMask & 0x01) != 0)
                    planes[0][address] = value;
                if ((mapMask & 0x02) != 0)
                    planes[1][address] = value;

                if ((mapMask & 0x04) != 0)
                    this.Font[(address / 32) * this.FontHeight + (address % 32)] = value;
            }
        }
    }
    internal override ushort GetVramWord(uint offset)
    {
        uint value = GetVramByte(offset);
        return (ushort)(value | (uint)(GetVramByte(offset + 1u) << 8));
    }
    internal override void SetVramWord(uint offset, ushort value)
    {
        this.SetVramByte(offset, (byte)value);
        this.SetVramByte(offset + 1u, (byte)(value >> 8));
    }
    internal override uint GetVramDWord(uint offset)
    {
        uint value = GetVramByte(offset);
        value |= (uint)(GetVramByte(offset + 1u) << 8);
        value |= (uint)(GetVramByte(offset + 2u) << 16);
        value |= (uint)(GetVramByte(offset + 3u) << 24);
        return value;
    }
    internal override void SetVramDWord(uint offset, uint value)
    {
        this.SetVramByte(offset, (byte)value);
        this.SetVramByte(offset + 1u, (byte)(value >> 8));
        this.SetVramByte(offset + 2u, (byte)(value >> 16));
        this.SetVramByte(offset + 3u, (byte)(value >> 24));
    }
    internal override void WriteCharacter(int x, int y, int index, byte foreground, byte background)
    {
        int value = index | (foreground << 8) | (background << 12);
        SetVramWord((uint)((y * this.Stride) + (x * 2)) + BaseAddress, (ushort)value);
    }
    internal override void InitializeMode(VideoHandler video)
    {
        base.InitializeMode(video);
        this.graphics.GraphicsMode = 0x10; // OddEven mode
        this.graphics.MiscellaneousGraphics = 0xE0; // OddEven mode
        this.sequencer.SequencerMemoryMode = SequencerMemoryMode.ExtendedMemory;
        this.sequencer.MapMask = 0x03;
    }
    /// <summary>
    /// Clears all of the characters and attributes on the active display page.
    /// </summary>
    internal void Clear()
    {
        var total = this.Width * this.Height;
        unsafe
        {
            for (int i = 0; i < total; i++)
            {
                this.planes[0][(DisplayPageSize * this.ActiveDisplayPage) + i] = 0;
                this.planes[1][(DisplayPageSize * this.ActiveDisplayPage) + i] = 0;
            }
        }
    }
    /// <summary>
    /// Clears a rectangle in the active display page.
    /// </summary>
    /// <param name="offset">Top left corner of the rectangle to clear.</param>
    /// <param name="width">Width of the rectangle to clear.</param>
    /// <param name="height">Height of the rectangle to clear.</param>
    public void Clear(Point offset, int width, int height)
    {
        if (width <= 0 || height <= 0)
            return;

        int pageOffset = DisplayPageSize * this.ActiveDisplayPage;

        int y2 = Math.Min(offset.Y + height, this.Height - 1);
        int x2 = Math.Min(offset.X + width, this.Width - 1);

        unsafe
        {
            for (int y = offset.Y; y < y2; y++)
            {
                for (int x = offset.X; x < x2; x++)
                {
                    int byteOffset = y * this.Width + x2;

                    this.planes[0][pageOffset + byteOffset] = 0;
                    this.planes[1][pageOffset + byteOffset] = 0;
                }
            }
        }
    }
    /// <summary>
    /// Copies a block of text in the console from one location to another
    /// and clears the source rectangle.
    /// </summary>
    /// <param name="sourceOffset">Top left corner of source rectangle to copy.</param>
    /// <param name="destinationOffset">Top left corner of destination rectangle to copy to.</param>
    /// <param name="width">Width of rectangle to copy.</param>
    /// <param name="height">Height of rectangle to copy.</param>
    /// <param name="backgroundCharacter">Character to fill in the source rectangle.</param>
    /// <param name="backgroundAttribute">Attribute to fill in the source rectangle.</param>
    internal void MoveBlock(Point sourceOffset, Point destinationOffset, int width, int height, byte backgroundCharacter, byte backgroundAttribute)
    {
        byte[,] charBuffer = new byte[height, width];
        byte[,] attrBuffer = new byte[height, width];

        int pageOffset = DisplayPageSize * this.ActiveDisplayPage;

        unsafe
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int offset = (sourceOffset.Y + y) * this.Width + sourceOffset.X + x;

                    charBuffer[y, x] = this.planes[0][pageOffset + offset];
                    attrBuffer[y, x] = this.planes[1][pageOffset + offset];

                    this.planes[0][pageOffset + offset] = backgroundCharacter;
                    this.planes[1][pageOffset + offset] = backgroundAttribute;
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (x + destinationOffset.X >= 0 && y + destinationOffset.Y >= 0)
                    {
                        int offset = (destinationOffset.Y + y) * this.Width + destinationOffset.X + x;

                        this.planes[0][pageOffset + offset] = charBuffer[y, x];
                        this.planes[1][pageOffset + offset] = attrBuffer[y, x];
                    }
                }
            }
        }
    }
    /// <summary>
    /// Returns the character at the specified coordinates.
    /// </summary>
    /// <param name="x">Horizontal character coordinate.</param>
    /// <param name="y">Vertical character coordinate.</param>
    /// <returns>Character and attribute at this specified position.</returns>
    internal ushort GetCharacter(int x, int y)
    {
        int pageOffset = DisplayPageSize * this.ActiveDisplayPage;

        unsafe
        {
            int offset = y * this.Width + x;
            return (ushort)(this.planes[0][pageOffset + offset] | (this.planes[1][pageOffset + offset] << 8));
        }
    }
    /// <summary>
    /// Scrolls lines of text up in a rectangle on the active display page.
    /// </summary>
    /// <param name="x1">Left coordinate of scroll region.</param>
    /// <param name="y1">Top coordinate of scroll region.</param>
    /// <param name="x2">Right coordinate of scroll region.</param>
    /// <param name="y2">Bottom coordinate of scroll region.</param>
    /// <param name="lines">Number of lines to scroll.</param>
    /// <param name="backgroundAttribute">Attribute to fill in bottom rows.</param>
    public void ScrollUp(int x1, int y1, int x2, int y2, int lines, byte backgroundAttribute)
    {
        int pageOffset = DisplayPageSize * this.ActiveDisplayPage;

        unsafe
        {
            for (int l = 0; l < lines; l++)
            {
                for (int y = y2 - 1; y >= y1; y--)
                {
                    for (int x = x1; x <= x2; x++)
                    {
                        int destOffset = pageOffset + y * this.Width + x;
                        int srcOffset = pageOffset + (y + 1) * this.Width + x;

                        this.planes[0][destOffset] = this.planes[0][srcOffset];
                        this.planes[1][destOffset] = this.planes[1][srcOffset];
                    }
                }

                for (int x = x1; x <= x2; x++)
                {
                    int destOffset = pageOffset + y2 * Width + x;

                    this.planes[0][destOffset] = 0;
                    this.planes[1][destOffset] = backgroundAttribute;
                }
            }
        }

        //Point srcOffset = new Point(x1, y1);
        //Point destOffset = new Point(x1, y1 - lines);
        //int width = Math.Abs(x2 - x1 + 1);
        //int height = Math.Abs(y2 - y1 + 1);

        //MoveBlock(srcOffset, destOffset, width, height, background);
        //CursorPosition = new Point(x1, y2);
    }
}
