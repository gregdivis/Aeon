namespace Aeon.Emulator.Video.Rendering;

internal sealed class VideoRenderer2Bit<TPixelFormat>(VideoMode mode) : VideoRenderer(mode)
    where TPixelFormat : IOutputPixelFormat
{
    private readonly uint[] palette =
    [
        0,
        TPixelFormat.FromBGRA(0x0000FFFF),
        TPixelFormat.FromBGRA(0x00FF00FF),
        TPixelFormat.FromBGRA(0x00FFFFFF)
    ];

    protected override void RenderFrame(Span<uint> destination)
    {
        int width = this.Mode.Width;
        int height = this.Mode.Height;
        int stride = this.Mode.Stride;

        var srcPtr = new UnsafePointer<byte>(this.Mode.VideoRamSpan);
        var destPtr = new UnsafePointer<uint>(destination);
        var palette = new UnsafePointer<uint>(this.palette);

        for (int y = 0; y < height; y += 2)
        {
            var srcRow = srcPtr + (stride * (y / 2));
            uint srcBit = 0;

            for (int x = 0; x < width; x++)
            {
                uint srcByte = srcRow[(int)(srcBit / 8)];
                uint shift = 6 - (srcBit % 8);

                uint c = Intrinsics.ExtractBits(srcByte, (byte)shift, 2, 0b11u << (int)shift);

                destPtr[(y * width) + x] = palette[(int)c];
                srcBit += 2;
            }
        }

        for (int y = 1; y < height; y += 2)
        {
            var srcRow = srcPtr + 8192 + (stride * (y / 2));
            uint srcBit = 0;

            for (int x = 0; x < width; x++)
            {
                uint srcByte = srcRow[(int)(srcBit / 8)];
                uint shift = 6 - (srcBit % 8);

                uint c = Intrinsics.ExtractBits(srcByte, (byte)shift, 2, 0b11u << (int)shift);

                destPtr[(y * width) + x] = palette[(int)c];
                srcBit += 2;
            }
        }
    }
}
