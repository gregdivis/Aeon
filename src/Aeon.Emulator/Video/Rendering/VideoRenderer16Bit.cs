namespace Aeon.Emulator.Video.Rendering;

internal sealed class VideoRenderer16Bit<TPixelFormat>(VideoMode mode) : VideoRenderer(mode)
    where TPixelFormat : IOutputPixelFormat
{
    protected override void RenderFrame(Span<uint> destination)
    {
        int totalPixels = this.Mode.Width * this.Mode.Height;

        unsafe
        {
            ushort* srcPtr = (ushort*)((byte*)this.Mode.VideoRam.ToPointer() + this.Mode.StartOffset);
            var destPtr = new UnsafePointer<uint>(destination);

            for (int i = 0; i < totalPixels; i++)
                destPtr[i] = TPixelFormat.FromRGB16(srcPtr[i]);
        }
    }
}
