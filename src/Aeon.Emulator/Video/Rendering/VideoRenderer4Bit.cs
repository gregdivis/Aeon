using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Video.Rendering;

internal sealed class VideoRenderer4Bit<TPixelFormat>(VideoMode mode) : PalettizedVideoRenderer<TPixelFormat>(mode)
    where TPixelFormat : IOutputPixelFormat
{
    protected override void RenderFrame(UnsafePointer<uint> palette, Span<uint> destination)
    {
        int width = this.Mode.Width;
        int height = Math.Min(this.Mode.Height, this.Mode.LineCompare + 1);
        int stride = this.Mode.Stride;
        int horizontalPan = this.Mode.HorizontalPanning;
        int startOffset = this.Mode.StartOffset;

        int safeWidth = Math.Min(stride, width / 8);
        int bitPan = horizontalPan % 8;

        unsafe
        {
            var srcPtr = new UnsafePointer<uint>(this.Mode.VideoRamSpan);
            var destPtr = new UnsafePointer<uint>(destination);

            var paletteMap = new UnsafePointer<byte>(this.Mode.InternalPalette);
            int destStart = 0;

            for (int split = 0; split < 2; split++)
            {
                for (int y = 0; y < height; y++)
                {
                    int srcPos = (stride * y + startOffset + horizontalPan / 8) & 0xFFFF;
                    int destPos = width * y + destStart;

                    for (int i = bitPan; i < 8; i++)
                        destPtr[destPos++] = palette[paletteMap[UnpackIndex(srcPtr[srcPos], 7 - i)]];

                    srcPos++;

                    for (int xb = 1; xb < safeWidth; xb++)
                    {
                        // vram is stored as:
                        // [p1byte] [p2byte] [p3byte] [p4byte]
                        // to build index for nibble one:
                        // p1[0] p2[0] p3[0] p4[0]

                        uint p = srcPtr[srcPos & 0xFFFF];
                        int palIndex = UnpackIndex(p, 0);
                        destPtr[destPos + 7] = palette[paletteMap[palIndex]];

                        palIndex = UnpackIndex(p, 1);
                        destPtr[destPos + 6] = palette[paletteMap[palIndex]];

                        palIndex = UnpackIndex(p, 2);
                        destPtr[destPos + 5] = palette[paletteMap[palIndex]];

                        palIndex = UnpackIndex(p, 3);
                        destPtr[destPos + 4] = palette[paletteMap[palIndex]];

                        palIndex = UnpackIndex(p, 4);
                        destPtr[destPos + 3] = palette[paletteMap[palIndex]];

                        palIndex = UnpackIndex(p, 5);
                        destPtr[destPos + 2] = palette[paletteMap[palIndex]];

                        palIndex = UnpackIndex(p, 6);
                        destPtr[destPos + 1] = palette[paletteMap[palIndex]];

                        palIndex = UnpackIndex(p, 7);
                        destPtr[destPos] = palette[paletteMap[palIndex]];

                        destPos += 8;
                        srcPos++;
                    }

                    srcPos &= 0xFFFF;

                    for (int i = 0; i < bitPan; i++)
                        destPtr[destPos++] = palette[paletteMap[UnpackIndex(srcPtr[srcPos], 7 - i)]];
                }

                if (height < this.Mode.Height)
                {
                    startOffset = 0;
                    height = this.Mode.Height - this.Mode.LineCompare - 1;
                    destStart = this.Mode.LineCompare * width;
                }
                else
                {
                    break;
                }
            }
        }
    }

    // it's important for this to get inlined
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int UnpackIndex(uint value, int index)
    {
        if (System.Runtime.Intrinsics.X86.Bmi2.IsSupported)
            return (int)System.Runtime.Intrinsics.X86.Bmi2.ParallelBitExtract(value, 0x01010101u << index);
        else
            return (int)(((value & (1u << index)) >> index) | ((value & (0x100u << index)) >> (7 + index)) | ((value & (0x10000u << index)) >> (14 + index)) | ((value & (0x1000000u << index)) >> (21 + index)));
    }
}
