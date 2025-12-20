using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Video.Modes;

internal sealed class CgaMode4 : VideoMode
{
    private const uint BaseAddress = 0x18000;

    public CgaMode4(VideoHandler video) : base(320, 200, 2, false, 8, VideoModeType.Graphics, video)
    {
    }

    public override int Stride => 80;

    internal override byte GetVramByte(uint offset)
    {
        offset -= BaseAddress;
        return this.VideoRamSpan[(int)offset];
    }
    internal override void SetVramByte(uint offset, byte value)
    {
        offset -= BaseAddress;
        this.VideoRamSpan[(int)offset] = value;
    }
    internal override ushort GetVramWord(uint offset)
    {
        offset -= BaseAddress;
        return Unsafe.As<byte, ushort>(ref this.VideoRamSpan[(int)offset]);
    }
    internal override void SetVramWord(uint offset, ushort value)
    {
        offset -= BaseAddress;
        Unsafe.As<byte, ushort>(ref this.VideoRamSpan[(int)offset]) = value;
    }
    internal override uint GetVramDWord(uint offset)
    {
        offset -= BaseAddress;
        return Unsafe.As<byte, uint>(ref this.VideoRamSpan[(int)offset]);
    }
    internal override void SetVramDWord(uint offset, uint value)
    {
        offset -= BaseAddress;
        Unsafe.As<byte, uint>(ref this.VideoRamSpan[(int)offset]) = value;
    }
    internal override void WriteCharacter(int x, int y, int index, byte foreground, byte background)
    {
        throw new NotImplementedException("WriteCharacter in CGA.");
    }
}
