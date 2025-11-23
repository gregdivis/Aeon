namespace Aeon.Emulator.Dos.VirtualFileSystem;

/// <summary>
/// Specifies additional properties for magnetic media.
/// </summary>
public interface IMagneticDrive
{
    /// <summary>
    /// Gets the number of cylinders on the drive.
    /// </summary>
    /// <remarks>
    /// This value must be between 1 and 1024 (inclusive).
    /// </remarks>
    int Cylinders { get; }
    /// <summary>
    /// Gets the number of heads on the drive.
    /// </summary>
    /// <remarks>
    /// This value must be between 0 and 255 (inclusive).
    /// </remarks>
    int Heads { get; }
    /// <summary>
    /// Gets the number of sectors per track on the drive.
    /// </summary>
    /// <remarks>
    /// This value must be between 0 and 63 (inclusive).
    /// </remarks>
    int Sectors { get; }
    /// <summary>
    /// Gets the size of a sector in bytes.
    /// </summary>
    int BytesPerSector { get; }
    /// <summary>
    /// Gets the size of a cluster in sectors.
    /// </summary>
    int SectorsPerCluster { get; }
    /// <summary>
    /// Gets the number of clusters on the drive.
    /// </summary>
    int Clusters { get; }
}
