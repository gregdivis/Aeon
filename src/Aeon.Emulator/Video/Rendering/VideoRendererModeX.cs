using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Video.Rendering;

internal sealed class VideoRendererModeX<TPixelFormat>(VideoMode mode) : PalettizedVideoRenderer<TPixelFormat>(mode)
    where TPixelFormat : IOutputPixelFormat
{
    protected override void RenderFrame(UnsafePointer<uint> palette, Span<uint> destination)
    {
        int width = this.Mode.Width;
        int height = this.Mode.Height;
        int startOffset = this.Mode.StartOffset;
        int stride = this.Mode.Stride;
        int lineCompare = this.Mode.LineCompare / 2;

        var destPtr = new UnsafePointer<uint>(destination);
        var src = new UnsafePointer<uint>(this.Mode.VideoRamSpan);

        int max = Math.Min(height, lineCompare + 1);
        int wordWidth = width / 4;

        for (int y = 0; y < max; y++)
        {
            int srcPos = (y * stride) + startOffset;
            int destPos = y * width;
            ReadRow(src, destPtr + destPos, srcPos, wordWidth, palette);
        }

        if (max < height)
        {
            for (int y = max + 1; y < height; y++)
            {
                int srcPos = (y - max) * stride;
                int destPos = y * width;
                ReadRow(src, destPtr + destPos, srcPos, wordWidth, palette);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void ReadRow(UnsafePointer<uint> src, UnsafePointer<uint> dest, int srcPos, int wordWidth, UnsafePointer<uint> palette)
        {
            for (int x = 0; x < wordWidth; x++)
            {
                uint p = src[(srcPos + x) & ushort.MaxValue];
                dest.Value = palette[p & 0xFF];
                dest++;
                dest.Value = palette[(p >>> 8) & 0xFF];
                dest++;
                dest.Value = palette[(p >>> 16) & 0xFF];
                dest++;
                dest.Value = palette[(p >>> 24) & 0xFF];
                dest++;
            }
        }
    }
}
