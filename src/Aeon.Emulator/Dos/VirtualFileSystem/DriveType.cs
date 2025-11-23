namespace Aeon.Emulator;

/// <summary>
/// Specifies the type of a virtual drive.
/// </summary>
public enum DriveType
{
    /// <summary>
    /// No drive is present.
    /// </summary>
    None,
    /// <summary>
    /// A 3.5" floppy drive is present.
    /// </summary>
    Floppy35,
    /// <summary>
    /// A 5.25" floppy drive is present.
    /// </summary>
    Floppy525,
    /// <summary>
    /// A hard drive is present.
    /// </summary>
    Fixed,
    /// <summary>
    /// A CD-ROM drive is present.
    /// </summary>
    CDROM
}
