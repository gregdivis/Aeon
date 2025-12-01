using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using Aeon.Emulator.Instructions;

namespace Aeon.Emulator.Video.Rendering;

public readonly struct PixelFormatRGBA : IOutputPixelFormat
{
    public static void ConvertBGRAPalette(ReadOnlySpan<uint> bgraPalette, Span<uint> outputPalette)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(bgraPalette.Length, 256);
        ArgumentOutOfRangeException.ThrowIfNotEqual(outputPalette.Length, 256);

        if (Vector512.IsHardwareAccelerated)
        {
            ref Vector512<byte> src = ref Unsafe.As<uint, Vector512<byte>>(ref MemoryMarshal.GetReference(bgraPalette));
            ref Vector512<byte> dest = ref Unsafe.As<uint, Vector512<byte>>(ref MemoryMarshal.GetReference(outputPalette));
            int count = 16;
            var indexes = Vector512.Create(BitShufflePattern);

            while (count-- > 0)
            {
                dest = Vector512.Shuffle(src, indexes);
                dest = ref Unsafe.Add(ref dest, 1);
                src = ref Unsafe.Add(ref src, 1);
            }
        }
        else if (Vector256.IsHardwareAccelerated)
        {
            ref Vector256<byte> src = ref Unsafe.As<uint, Vector256<byte>>(ref MemoryMarshal.GetReference(bgraPalette));
            ref Vector256<byte> dest = ref Unsafe.As<uint, Vector256<byte>>(ref MemoryMarshal.GetReference(outputPalette));
            int count = 32;
            var indexes = Vector256.Create(BitShufflePattern);

            while (count-- > 0)
            {
                dest = Vector256.Shuffle(src, indexes);
                dest = ref Unsafe.Add(ref dest, 1);
                src = ref Unsafe.Add(ref src, 1);
            }
        }
        else if (Vector128.IsHardwareAccelerated)
        {
            ref Vector128<byte> src = ref Unsafe.As<uint, Vector128<byte>>(ref MemoryMarshal.GetReference(bgraPalette));
            ref Vector128<byte> dest = ref Unsafe.As<uint, Vector128<byte>>(ref MemoryMarshal.GetReference(outputPalette));
            int count = 64;
            var indexes = Vector128.Create(BitShufflePattern);

            while (count-- > 0)
            {
                dest = Vector128.Shuffle(src, indexes);
                dest = ref Unsafe.Add(ref dest, 1);
                src = ref Unsafe.Add(ref src, 1);
            }
        }
        else
        {
            ref uint src = ref MemoryMarshal.GetReference(bgraPalette);
            ref uint dest = ref MemoryMarshal.GetReference(outputPalette);
            int count = 256;

            while (count-- > 0)
            {
                uint r = (src >>> 16) & 0xFFu;
                uint b = src & 0xFFu;
                dest = (src & 0xFF00FF00u) | r | (b << 16);
                dest = ref Unsafe.Add(ref dest, 1);
                src = ref Unsafe.Add(ref src, 1);
            }
        }
    }

    public static uint FromRGB16(ushort value)
    {
        throw new NotImplementedException();
    }

    [StructLayout(LayoutKind.Sequential, Size = 4)]
    private readonly struct Bgra
    {
        public Bgra(byte r, byte g, byte b, byte a)
        {
            this.Red = r;
            this.Green = g;
            this.Blue = b;
            this.Alpha = a;
        }

        public readonly byte Red;
        public readonly byte Green;
        public readonly byte Blue;
        public readonly byte Alpha;
    }

    public static void SwapRedBlue(Span<byte> bgraBuffer)
    {
        if (Vector512.IsHardwareAccelerated && bgraBuffer.Length >= Vector512<byte>.Count)
        {
            var indexes = Vector512.Create(BitShufflePattern);
            var vectorBuffer = MemoryMarshal.Cast<byte, Vector512<int>>(bgraBuffer);
            for (int i = 0; i < vectorBuffer.Length; i++)
                vectorBuffer[i] = Vector512.Shuffle(vectorBuffer[i].AsByte(), indexes).AsInt32();
        }
        else if (Vector256.IsHardwareAccelerated && bgraBuffer.Length >= Vector256<byte>.Count)
        {
            var indexes = Vector256.Create(BitShufflePattern);
            var vectorBuffer = MemoryMarshal.Cast<byte, Vector256<int>>(bgraBuffer);
            for (int i = 0; i < vectorBuffer.Length; i++)
                vectorBuffer[i] = Vector256.Shuffle(vectorBuffer[i].AsByte(), indexes).AsInt32();
        }
        else if (Vector128.IsHardwareAccelerated && bgraBuffer.Length >= Vector128<byte>.Count)
        {
            var indexes = Vector128.Create(BitShufflePattern);
            var vectorBuffer = MemoryMarshal.Cast<byte, Vector128<int>>(bgraBuffer);
            for (int i = 0; i < vectorBuffer.Length; i++)
                vectorBuffer[i] = Vector128.Shuffle(vectorBuffer[i].AsByte(), indexes).AsInt32();
        }
        else
        {
            var uintBuffer = MemoryMarshal.Cast<byte, Bgra>(bgraBuffer);
            for (int i = 0; i < uintBuffer.Length; i++)
            {
                var value = uintBuffer[i];
                uintBuffer[i] = new Bgra(value.Blue, value.Green, value.Red, value.Alpha);
            }
        }
    }

    public static uint FromBGRA(uint value)
    {
        uint r = (value >>> 16) & 0xFFu;
        uint b = value & 0xFFu;
        return (value & 0xFF00FF00u) | r | (b << 16);
    }

    private static ReadOnlySpan<byte> BitShufflePattern =>
    [
        2, 1, 0, 3,
        6, 5, 4, 7,
        10, 9, 8, 11,
        14, 13, 12, 15,
        18, 17, 16, 19,
        22, 21, 20, 23,
        26, 25, 24, 27,
        30, 29, 28, 31,
        34, 33, 32, 35,
        38, 37, 36, 39,
        42, 41, 40, 43,
        46, 45, 44, 47,
        50, 49, 48, 51,
        54, 53, 52, 55,
        58, 57, 56, 59,
        62, 61, 60, 63
    ];
}
