using System.Runtime.InteropServices;

namespace Aeon.Emulator.Video.Rendering;

public sealed class MemoryBitmap : IDisposable
{
    private unsafe void* data;

    public MemoryBitmap(int width, int height)
    {
        this.Width = width;
        this.Height = height;
        unsafe
        {
            this.data = NativeMemory.AlignedAlloc((nuint)(width * height * sizeof(uint)), sizeof(uint));
        }
    }
    ~MemoryBitmap() => this.Dispose(false);

    public int Width { get; }
    public int Height { get; }
    public IntPtr PixelBuffer
    {
        get
        {
            unsafe
            {
                return new IntPtr(this.data);
            }
        }
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        unsafe
        {
            if (this.data != null)
            {
                NativeMemory.AlignedFree(this.data);
                this.data = null;
            }
        }
    }
}
