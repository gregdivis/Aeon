namespace Aeon.Emulator;

/// <summary>
/// Specifies one of the int 10h video modes.
/// </summary>
internal enum VideoMode10
{
    /// <summary>
    /// Monochrome 40x25 text mode.
    /// </summary>
    Text40x25x1 = 0x00,
    /// <summary>
    /// Color 40x25 text mode (4-bit).
    /// </summary>
    ColorText40x25x4 = 0x01,
    /// <summary>
    /// Monochrome 80x25 text mode (4-bit).
    /// </summary>
    MonochromeText80x25x4 = 0x02,
    /// <summary>
    /// Color 80x25 text mode (4-bit).
    /// </summary>
    ColorText80x25x4 = 0x03,
    /// <summary>
    /// Color 320x200 graphics mode (2-bit).
    /// </summary>
    ColorGraphics320x200x2A = 0x04,
    /// <summary>
    /// Color 320x200 graphics mode (2-bit).
    /// </summary>
    ColorGraphics320x200x2B = 0x05,
    /// <summary>
    /// Monochrome 640x200 graphics mode (1-bit).
    /// </summary>
    Graphics640x200x1 = 0x06,
    /// <summary>
    /// Monochrome 80x25 text mode (1-bit).
    /// </summary>
    Text80x25x1 = 0x07,
    /// <summary>
    /// Color 320x200 graphics mode (4-bit).
    /// </summary>
    ColorGraphics320x200x4 = 0x0D,
    /// <summary>
    /// Color 640x200 graphics mode (4-bit).
    /// </summary>
    ColorGraphics640x200x4 = 0x0E,
    /// <summary>
    /// Monochrome 640x350 graphics mode (1-bit).
    /// </summary>
    Graphics640x350x1 = 0x0F,
    /// <summary>
    /// Color 640x350 graphics mode (4-bit).
    /// </summary>
    ColorGraphics640x350x4 = 0x10,
    /// <summary>
    /// Monochrome 640x480 graphics mode (1-bit).
    /// </summary>
    Graphics640x480x1 = 0x11,
    /// <summary>
    /// Color 640x480 graphics mode (4-bit).
    /// </summary>
    Graphics640x480x4 = 0x12,
    /// <summary>
    /// Color 320x200 graphics mode (8-bit).
    /// </summary>
    Graphics320x200x8 = 0x13
}
