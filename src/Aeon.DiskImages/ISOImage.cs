using Aeon.DiskImages.Iso9660;
using Aeon.Emulator;
using Aeon.Emulator.Dos;
using Aeon.Emulator.Dos.VirtualFileSystem;

namespace Aeon.DiskImages;

/// <summary>
/// Provides access to an ISO CD-ROM image file.
/// </summary>
public sealed class ISOImage : IMappedDrive, IRawSectorReader
{
    private readonly Stream fileStream;
    private readonly Iso9660Disc disc;

    /// <summary>
    /// Initializes a new instance of the ISOImage class.
    /// </summary>
    /// <param name="isoFilePath">Full path to the ISO image file to read.</param>
    public ISOImage(string isoFilePath)
    {
        this.fileStream = File.OpenRead(isoFilePath);
        this.disc = new Iso9660Disc(this.fileStream);
    }

    /// <summary>
    /// Gets the volume label of the mapped drive.
    /// </summary>
    public string VolumeLabel => this.disc.PrimaryVolumeDescriptor.VolumeIdentifier;
    /// <summary>
    /// The size of each sector in bytes.
    /// </summary>
    public int SectorSize => 2048;
    public long FreeSpace => 0;

    /// <summary>
    /// Opens a file for read access.
    /// </summary>
    /// <param name="path">Path to file.</param>
    /// <returns>Stream back by specified file.</returns>
    public ErrorCodeResult<Stream> OpenRead(VirtualPath path)
    {
        ArgumentNullException.ThrowIfNull(path);

        var entry = this.disc.GetDirectoryEntry(path.Elements);
        if (entry == null)
            return ExtendedErrorCode.FileNotFound;

        return this.disc.Open(entry);
    }
    /// <summary>
    /// Returns a collection of files contained in the specified directory.
    /// </summary>
    /// <param name="path">Directory whose content is returned.</param>
    /// <returns>Collection of files contained in the specified directory.</returns>
    public ErrorCodeResult<IEnumerable<VirtualFileInfo>> GetDirectory(VirtualPath path)
    {
        ArgumentNullException.ThrowIfNull(path);

        var entry = this.disc.GetDirectoryEntry(path.Elements);
        if (entry == null)
            return ExtendedErrorCode.PathNotFound;

        return new ErrorCodeResult<IEnumerable<VirtualFileInfo>>(entry.Children);
    }
    /// <summary>
    /// Returns information about a specific file or directory.
    /// </summary>
    /// <param name="path">Path of file or directory to get information for.</param>
    /// <returns>Information about the specified file or directory; null if file was not found.</returns>
    public ErrorCodeResult<VirtualFileInfo> GetFileInfo(VirtualPath path)
    {
        ArgumentNullException.ThrowIfNull(path);

        var info = this.disc.GetDirectoryEntry(path.Elements);
        if (info != null)
            return info;

        return ExtendedErrorCode.FileNotFound;
    }
    public void ReadSectors(int startingSector, int sectorsToRead, Span<byte> buffer) => this.disc.ReadRaw(startingSector, sectorsToRead, buffer);
    /// <summary>
    /// Releases resources used by the drive.
    /// </summary>
    public void Dispose()
    {
        this.disc.Dispose();
        this.fileStream.Dispose();
    }
}
