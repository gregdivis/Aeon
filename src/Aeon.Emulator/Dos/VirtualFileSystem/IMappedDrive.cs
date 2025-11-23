namespace Aeon.Emulator.Dos.VirtualFileSystem;

/// <summary>
/// Describes a file system mapping that provides read access to its contents.
/// </summary>
public interface IMappedDrive : IDisposable
{
    /// <summary>
    /// Gets the volume label of the mapped drive.
    /// </summary>
    string? VolumeLabel { get; }
    /// <summary>
    /// Gets the amount of free space on the drive.
    /// </summary>
    long FreeSpace { get; }

    /// <summary>
    /// Opens a file for read access.
    /// </summary>
    /// <param name="path">Path to file.</param>
    /// <returns>Stream backed by specified file.</returns>
    ErrorCodeResult<Stream> OpenRead(VirtualPath path);
    /// <summary>
    /// Returns a collection of files contained in the specified directory.
    /// </summary>
    /// <param name="path">Directory whose content is returned.</param>
    /// <returns>Collection of files contained in the specified directory.</returns>
    ErrorCodeResult<IEnumerable<VirtualFileInfo>> GetDirectory(VirtualPath path);
    /// <summary>
    /// Returns information about a specific file or directory.
    /// </summary>
    /// <param name="path">Path of file or directory to get information for.</param>
    /// <returns>Information about the specified file or directory; null if file was not found.</returns>
    ErrorCodeResult<VirtualFileInfo> GetFileInfo(VirtualPath path);
}
