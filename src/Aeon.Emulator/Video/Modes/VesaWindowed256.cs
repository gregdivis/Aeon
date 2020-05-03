namespace Aeon.Emulator.Video.Modes
{
    /// <summary>
    /// A windowed 256-color VESA mode.
    /// </summary>
    internal sealed class VesaWindowed256 : VesaWindowed
    {
        public VesaWindowed256(int width, int height, VideoHandler video)
            : base(width, height, 8, false, 16, VideoModeType.Graphics, video)
        {
        }
    }
}
