namespace Aeon.Emulator.Video.Modes
{
    /// <summary>
    /// A windowed 16-bit-color VESA mode.
    /// </summary>
    internal sealed class VesaWindowed16Bit : VesaWindowed
    {
        public VesaWindowed16Bit(int width, int height, VideoHandler video)
            : base(width, height, 16, false, 16, VideoModeType.Graphics, video)
        {
        }
    }
}
