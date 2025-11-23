namespace Aeon.Emulator.Video.Modes;

/// <summary>
/// Provides functionality for planar 256-color VGA modes.
/// </summary>
internal sealed class Unchained256(int width, int height, VideoHandler video) : Planar4(width, height, 8, 8, VideoModeType.Graphics, video)
{
}
