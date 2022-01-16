using System;
using System.Numerics;

namespace Aeon.Emulator.Video.Rendering
{
    internal abstract class Scaler
    {
        protected Scaler(int width, int height)
        {
            this.SourceWidth = width;
            this.SourceHeight = height;
        }

        public int SourceWidth { get; }
        public int SourceHeight { get; }
        public abstract int TargetWidth { get; }
        public abstract int TargetHeight { get; }
        public int WidthRatio => this.TargetWidth / this.SourceWidth;
        public int HeightRatio => this.TargetHeight / this.SourceHeight;

        public void Apply(IntPtr source, IntPtr destination)
        {
            if (Vector.IsHardwareAccelerated)
                this.VectorScale(source, destination);
            else
                this.Scale(source, destination);
        }

        protected abstract void Scale(IntPtr source, IntPtr destination);
        protected abstract void VectorScale(IntPtr source, IntPtr destination);
    }
}
