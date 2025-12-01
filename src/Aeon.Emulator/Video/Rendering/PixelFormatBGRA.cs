namespace Aeon.Emulator.Video.Rendering;

public readonly struct PixelFormatBGRA : IOutputPixelFormat
{
    public static void ConvertBGRAPalette(ReadOnlySpan<uint> bgraPalette, Span<uint> outputPalette) => bgraPalette.CopyTo(outputPalette);

    private const double RedRatio = 255.0 / 31.0;
    private const double GreenRatio = 255.0 / 63.0;
    private const double BlueRatio = 255.0 / 31.0;

    public static uint FromRGB16(ushort value)
    {
        uint r = (uint)(((value & 0xF800) >> 11) * RedRatio) & 0xFFu;
        uint g = (uint)(((value & 0x07E0) >> 5) * GreenRatio) & 0xFFu;
        uint b = (uint)((value & 0x001F) * BlueRatio) & 0xFFu;

        return (r << 16) | (g << 8) | b;
    }

    public static uint FromBGRA(uint value) => value;
}
