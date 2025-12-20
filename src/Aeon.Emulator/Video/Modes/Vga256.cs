using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Video.Modes;

/// <summary>
/// Implements functionality for chained 8-bit 256-color VGA modes.
/// </summary>
internal sealed class Vga256 : VideoMode
{
    public Vga256(int width, int height, VideoHandler video) : base(width, height, 8, false, 8, VideoModeType.Graphics, video)
    {
    }

    public override int MouseWidth => this.PixelWidth * 2;

    internal override byte GetVramByte(uint offset) => this.VideoRamSpan[(int)offset];
    internal override void SetVramByte(uint offset, byte value) => this.VideoRamSpan[(int)offset] = value;
    internal override ushort GetVramWord(uint offset) => Unsafe.As<byte, ushort>(ref this.VideoRamSpan[(int)offset]);
    internal override void SetVramWord(uint offset, ushort value) => Unsafe.As<byte, ushort>(ref this.VideoRamSpan[(int)offset]) = value;
    internal override uint GetVramDWord(uint offset) => Unsafe.As<byte, uint>(ref this.VideoRamSpan[(int)offset]);
    internal override void SetVramDWord(uint offset, uint value) => Unsafe.As<byte, uint>(ref this.VideoRamSpan[(int)offset]) = value;
    internal override void WriteCharacter(int x, int y, int index, byte foreground, byte background)
    {
        unsafe
        {
            int stride = this.Stride;
            int startPos = (y * stride * 8) + x * 8;
            byte[] font = this.Font;

            for (int row = 0; row < 8; row++)
            {
                uint value = font[index * 8 + row];
                int pos = startPos + (row * stride);

                for (int column = 0; column < 8; column++)
                    this.VideoRamSpan[pos + column] = (value & (0x80 >> column)) != 0 ? foreground : background;
            }
        }
    }
}
