using System;
using System.Runtime.InteropServices;

namespace Aeon.Emulator.Video.Rendering
{
    internal sealed class MemoryBitmap : IDisposable
    {
        private unsafe void* data;

        public MemoryBitmap(int width, int height)
        {
            unsafe
            {
                this.data = NativeMemory.AlignedAlloc((nuint)(width * height * sizeof(uint)), sizeof(uint));
            }
        }
        ~MemoryBitmap() => this.Dispose(false);

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
}
