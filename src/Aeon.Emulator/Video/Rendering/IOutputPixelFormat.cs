namespace Aeon.Emulator.Video.Rendering;

public interface IOutputPixelFormat
{
    static abstract uint FromRGB16(ushort value);
    static abstract void ConvertBGRAPalette(ReadOnlySpan<uint> bgraPalette, Span<uint> outputPalette);
    static abstract uint FromBGRA(uint value);
}
