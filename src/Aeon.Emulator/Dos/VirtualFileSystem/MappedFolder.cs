namespace Aeon.Emulator.Dos.VirtualFileSystem;

/// <summary>
/// Provides read access to a folder on the host system.
/// </summary>
public class MappedFolder : IMappedDrive
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MappedFolder"/> class.
    /// </summary>
    /// <param name="hostPath">Path of the mapped folder on the host system.</param>
    public MappedFolder(string hostPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(hostPath);
        if (!Path.IsPathRooted(hostPath))
            throw new ArgumentException("Path must be absolute.");

        this.HostPath = hostPath;
        this.VolumeLabel = Path.GetFileName(hostPath);
    }

    /// <summary>
    /// Gets the path of the mapped folder on the host system.
    /// </summary>
    public string HostPath { get; }
    /// <summary>
    /// Gets the volume label of the mapped drive.
    /// </summary>
    public string VolumeLabel { get; }
    public virtual long FreeSpace => 0;

    /// <summary>
    /// Converts host file info to DOS file info.
    /// </summary>
    /// <param name="info">Host file info to convert.</param>
    /// <returns>Equivalent DOS file info.</returns>
    public static VirtualFileInfo Convert(FileSystemInfo info)
    {
        var attributes = Convert(info.Attributes);
        string name = info.Name.ToUpperInvariant();

        if (!attributes.HasFlag(VirtualFileAttributes.Directory))
        {
            var fileInfo = (FileInfo)info;
            return new VirtualFileInfo(name, attributes, fileInfo.LastWriteTimeUtc, fileInfo.Length);
        }
        else
        {
            return new VirtualFileInfo(name, attributes, info.LastWriteTimeUtc, 0);
        }
    }
    /// <summary>
    /// Converts host file attributes to DOS file attributes.
    /// </summary>
    /// <param name="attributes">Host file attributes to convert.</param>
    /// <returns>Equivalent DOS file attributes.</returns>
    public static VirtualFileAttributes Convert(FileAttributes attributes)
    {
        var res = VirtualFileAttributes.Default;
        if (attributes.HasFlag(FileAttributes.Archive))
            res |= VirtualFileAttributes.Archived;
        if (attributes.HasFlag(FileAttributes.Directory))
            res |= VirtualFileAttributes.Directory;
        if (attributes.HasFlag(FileAttributes.Hidden))
            res |= VirtualFileAttributes.Hidden;
        if (attributes.HasFlag(FileAttributes.ReadOnly))
            res |= VirtualFileAttributes.ReadOnly;
        if (attributes.HasFlag(FileAttributes.System))
            res |= VirtualFileAttributes.System;
        return res;
    }

    /// <summary>
    /// Opens a file for read access.
    /// </summary>
    /// <param name="path">Path to file.</param>
    /// <returns>Stream back by specified file.</returns>
    public virtual ErrorCodeResult<Stream> OpenRead(VirtualPath path)
    {
        ArgumentNullException.ThrowIfNull(path);

        string fullPath = GetFullPath(path);

        if (File.Exists(fullPath))
            return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        return Directory.Exists(Path.GetDirectoryName(fullPath)) ? ExtendedErrorCode.FileNotFound : ExtendedErrorCode.PathNotFound;
    }
    /// <summary>
    /// Returns a collection of files contained in the specified directory.
    /// </summary>
    /// <param name="path">Directory whose content is returned.</param>
    /// <returns>Collection of files contained in the specified directory.</returns>
    /// <exception cref="System.IO.DirectoryNotFoundException">The specified directory was not found.</exception>
    public virtual ErrorCodeResult<IEnumerable<VirtualFileInfo>> GetDirectory(VirtualPath path)
    {
        ArgumentNullException.ThrowIfNull(path);

        string fullPath = GetFullPath(path);
        var directory = new DirectoryInfo(fullPath);

        if (!directory.Exists)
            return ExtendedErrorCode.PathNotFound;

        return new ErrorCodeResult<IEnumerable<VirtualFileInfo>>(from i in directory.GetFileSystemInfos("*")
                                                                 where VirtualDirectory.IsLegalDosName(i.Name)
                                                                 select Convert(i));
    }
    /// <summary>
    /// Returns information about a specific file or directory.
    /// </summary>
    /// <param name="path">Path of file or directory to get information for.</param>
    /// <returns>Information about the specified file or directory; null if file was not found.</returns>
    public virtual ErrorCodeResult<VirtualFileInfo> GetFileInfo(VirtualPath path)
    {
        ArgumentNullException.ThrowIfNull(path);

        string fullPath;
        try
        {
            fullPath = GetFullPath(path);
        }
        catch (ArgumentException)
        {
            return ExtendedErrorCode.FileNotFound;
        }

        if (!Directory.Exists(Path.GetDirectoryName(fullPath)))
            return ExtendedErrorCode.PathNotFound;

        FileSystemInfo? info = null;

        if (File.Exists(fullPath))
            info = new FileInfo(fullPath);
        else if (Directory.Exists(fullPath))
            info = new DirectoryInfo(fullPath);

        if (info == null || !VirtualDirectory.IsLegalDosName(info.Name))
            return ExtendedErrorCode.FileNotFound;

        return Convert(info);
    }
    /// <summary>
    /// Releases resources used by the MappedFolder.
    /// </summary>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases resources used by the instance.
    /// </summary>
    /// <param name="disposing">Value indicating whether method is being called from the Dispose method.</param>
    protected virtual void Dispose(bool disposing)
    {
    }
    /// <summary>
    /// Returns the full path of a specified file or folder.
    /// </summary>
    /// <param name="path">Relative path supplied by the virtual file system.</param>
    /// <returns>Full path to the specified file or folder.</returns>
    protected string GetFullPath(VirtualPath path)
    {
        ArgumentNullException.ThrowIfNull(path);
        return Path.Combine(this.HostPath, path.Path.Replace('\\', Path.DirectorySeparatorChar));
    }
}
