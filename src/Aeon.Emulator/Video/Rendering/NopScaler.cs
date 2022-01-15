using System;

namespace Aeon.Presentation.Rendering
{
    internal sealed class NopScaler : Scaler
    {
        public NopScaler(int width, int height)
            : base(width, height, new FastBitmap(width, height))
        {
        }

        public override int TargetWidth => this.SourceWidth;
        public override int TargetHeight => this.SourceHeight;
        public override IntPtr VideoModeRenderTarget => this.Output.PixelBuffer;

        protected override void Scale()
        {
        }
        protected override void VectorScale()
        {
        }
    }
}
