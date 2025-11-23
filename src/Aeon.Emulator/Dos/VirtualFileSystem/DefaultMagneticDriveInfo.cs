namespace Aeon.Emulator.Dos.VirtualFileSystem;

/// <summary>
/// Contains default magnetic drive information.
/// </summary>
internal sealed class DefaultMagneticDriveInfo : IMagneticDrive
{
    /// <summary>
    /// The default 3.5&quot; floppy drive configuration.
    /// </summary>
    public static readonly DefaultMagneticDriveInfo Floppy35 = new()
    {
        Cylinders = 80,
        Heads = 2,
        Sectors = 18,
        BytesPerSector = 512,
        SectorsPerCluster = 1
    };
    /// <summary>
    /// The default 2.25&quot; floppy drive configuration.
    /// </summary>
    public static readonly DefaultMagneticDriveInfo Floppy525 = new()
    {
        Cylinders = 80,
        Heads = 2,
        Sectors = 15,
        BytesPerSector = 512,
        SectorsPerCluster = 1
    };
    /// <summary>
    /// The default hard drive configuration.
    /// </summary>
    public static readonly DefaultMagneticDriveInfo Fixed = new()
    {
        Cylinders = 1024,
        Heads = 16,
        Sectors = 63,
        BytesPerSector = 512,
        SectorsPerCluster = 8
    };

    private DefaultMagneticDriveInfo()
    {
    }

    /// <summary>
    /// Gets the number of cylinders on the drive.
    /// </summary>
    public int Cylinders { get; private init; }
    /// <summary>
    /// Gets the number of heads on the drive.
    /// </summary>
    public int Heads { get; private init; }
    /// <summary>
    /// Gets the number of sectors per track on the drive.
    /// </summary>
    public int Sectors { get; private init; }
    /// <summary>
    /// Gets the size of a sector in bytes.
    /// </summary>
    public int BytesPerSector { get; private init; }
    /// <summary>
    /// Gets the size of a cluster in sectors.
    /// </summary>
    public int SectorsPerCluster { get; private init; }
    /// <summary>
    /// Gets the number of clusters on the drive.
    /// </summary>
    public int Clusters => (this.Cylinders * this.Sectors * this.Heads) / this.SectorsPerCluster;
}
