namespace Aeon.Emulator.Video.Rendering;

internal sealed class VideoRenderer8Bit<TPixelFormat>(VideoMode mode) : PalettizedVideoRenderer<TPixelFormat>(mode)
    where TPixelFormat : IOutputPixelFormat
{
    protected override void RenderFrame(UnsafePointer<uint> palette, Span<uint> destination)
    {
        int totalPixels = this.Mode.Width * this.Mode.Height;
        var src = new UnsafePointer<byte>(this.Mode.VideoRamSpan);
        var dest = new UnsafePointer<uint>(destination);

        for (int i = 0; i < totalPixels; i++)
        {
            dest.Value = palette[src.Value];
            src++;
            dest++;
        }
    }
}
