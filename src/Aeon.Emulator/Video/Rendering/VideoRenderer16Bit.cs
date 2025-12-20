namespace Aeon.Emulator.Video.Rendering;

internal sealed class VideoRenderer16Bit<TPixelFormat>(VideoMode mode) : VideoRenderer(mode)
    where TPixelFormat : IOutputPixelFormat
{
    protected override void RenderFrame(Span<uint> destination)
    {
        int totalPixels = this.Mode.Width * this.Mode.Height;

        var srcPtr = new UnsafePointer<ushort>(this.Mode.VideoRamSpan[this.Mode.StartOffset..]);
        var destPtr = new UnsafePointer<uint>(destination);

        for (int i = 0; i < totalPixels; i++)
            destPtr[i] = TPixelFormat.FromRGB16(srcPtr[i]);
    }
}
