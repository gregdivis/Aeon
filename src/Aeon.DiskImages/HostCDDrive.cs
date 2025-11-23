using Aeon.DiskImages.Iso9660;
using Aeon.Emulator;
using Aeon.Emulator.Dos;
using Aeon.Emulator.Dos.VirtualFileSystem;

namespace Aeon.DiskImages;

/// <summary>
/// Provides access to a physical CD-ROM drive in Aeon.
/// </summary>
public sealed class HostCDDrive : IMappedDrive
{
    private readonly RawCDReader hostDrive;
    private readonly Iso9660Disc currentDisc;

    /// <summary>
    /// Initializes a new instance of the <see cref="HostCDDrive"/> class.
    /// </summary>
    /// <param name="volume">The physical volume to access.</param>
    public HostCDDrive(string volume)
    {
        ArgumentNullException.ThrowIfNull(volume);

        this.hostDrive = new RawCDReader(volume);
        this.currentDisc = new Iso9660Disc(this.hostDrive);
    }

    /// <summary>
    /// Gets the volume label of the mapped drive.
    /// </summary>
    public string VolumeLabel => this.currentDisc.PrimaryVolumeDescriptor.VolumeIdentifier;
    public long FreeSpace => 0;

    /// <summary>
    /// Copies a CD to an ISO image file.
    /// </summary>
    /// <param name="sourceVolume">Volume to copy from.</param>
    /// <param name="imageFileName">Path to image file to create.</param>
    /// <param name="reportProgress">Optional delegate which is periodically invoked with the percent completion.</param>
    public static void CreateIsoImage(string sourceVolume, string imageFileName, Action<int>? reportProgress)
    {
        ArgumentNullException.ThrowIfNull(sourceVolume);
        ArgumentNullException.ThrowIfNull(imageFileName);

        using var reader = new RawCDReader(sourceVolume);
        using var writer = File.Create(imageFileName);
        var buffer = new byte[2048];
        uint count = (uint)(reader.Length / 2048);
        int currentPercent = 0;

        for (uint i = 0; i < count; i++)
        {
            reader.ReadExactly(buffer, 0, 2048);
            writer.Write(buffer, 0, 2048);

            if (reportProgress != null && currentPercent != (int)((double)i / (double)count))
            {
                currentPercent = (int)((double)i / (double)count);
                reportProgress(currentPercent);
            }
        }
    }
    /// <summary>
    /// Copies a CD to an ISO image file.
    /// </summary>
    /// <param name="sourceVolume">Volume to copy from.</param>
    /// <param name="imageFileName">Path to image file to create.</param>
    public static void CreateIsoImage(string sourceVolume, string imageFileName) => CreateIsoImage(sourceVolume, imageFileName, null);

    /// <summary>
    /// Opens a file for read access.
    /// </summary>
    /// <param name="path">Path to file.</param>
    /// <returns>
    /// Stream backed by specified file.
    /// </returns>
    public ErrorCodeResult<Stream> OpenRead(VirtualPath path)
    {
        ArgumentNullException.ThrowIfNull(path);

        var entry = this.currentDisc.GetDirectoryEntry(path.Elements);
        if (entry == null)
            return ExtendedErrorCode.FileNotFound;

        return this.currentDisc.Open(entry);
    }
    /// <summary>
    /// Returns a collection of files contained in the specified directory.
    /// </summary>
    /// <param name="path">Directory whose content is returned.</param>
    /// <returns>
    /// Collection of files contained in the specified directory.
    /// </returns>
    public ErrorCodeResult<IEnumerable<VirtualFileInfo>> GetDirectory(VirtualPath path)
    {
        ArgumentNullException.ThrowIfNull(path);

        var entry = this.currentDisc.GetDirectoryEntry(path.Elements);
        if (entry == null)
            return ExtendedErrorCode.PathNotFound;

        return new ErrorCodeResult<IEnumerable<VirtualFileInfo>>(entry.Children);
    }
    /// <summary>
    /// Returns information about a specific file or directory.
    /// </summary>
    /// <param name="path">Path of file or directory to get information for.</param>
    /// <returns>
    /// Information about the specified file or directory; null if file was not found.
    /// </returns>
    public ErrorCodeResult<VirtualFileInfo> GetFileInfo(VirtualPath path)
    {
        ArgumentNullException.ThrowIfNull(path);

        var entry = this.currentDisc.GetDirectoryEntry(path.Elements);
        if (entry != null)
            return entry;
        return ExtendedErrorCode.FileNotFound;
    }
    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        this.currentDisc.Dispose();
        this.hostDrive.Dispose();
    }
}
