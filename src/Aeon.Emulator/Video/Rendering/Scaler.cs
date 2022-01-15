using System;
using System.Numerics;

namespace Aeon.Presentation.Rendering
{
    internal abstract class Scaler : IDisposable
    {
        private bool disposed;

        protected Scaler(int width, int height, FastBitmap output)
        {
            this.SourceWidth = width;
            this.SourceHeight = height;
            this.Output = output;
        }

        public int SourceWidth { get; }
        public int SourceHeight { get; }
        public abstract int TargetWidth { get; }
        public abstract int TargetHeight { get; }
        public FastBitmap Output { get; }
        public abstract IntPtr VideoModeRenderTarget { get; }
        public int WidthRatio => this.TargetWidth / this.SourceWidth;
        public int HeightRatio => this.TargetHeight / this.SourceHeight;

        public void Apply()
        {
            if (Vector.IsHardwareAccelerated)
                this.VectorScale();
            else
                this.Scale();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.Dispose(true);
        }

        protected abstract void Scale();
        protected abstract void VectorScale();

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                    this.Output.Dispose();

                this.disposed = true;
            }
        }
    }
}
