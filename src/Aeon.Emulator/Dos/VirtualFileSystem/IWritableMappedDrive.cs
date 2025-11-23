namespace Aeon.Emulator.Dos.VirtualFileSystem;

/// <summary>
/// Describes a file system mapping that provides write access to its contents.
/// </summary>
public interface IWritableMappedDrive : IMappedDrive
{
    /// <summary>
    /// Creates a new file at the specified location, overwriting an existing file.
    /// </summary>
    /// <param name="path">Path of file to create.</param>
    /// <returns>Stream backed by the new file.</returns>
    ErrorCodeResult<Stream> CreateFile(VirtualPath path);
    /// <summary>
    /// Opens an existing file for write access.
    /// </summary>
    /// <param name="path">Path of file to open.</param>
    /// <returns>Stream backed by the file.</returns>
    ErrorCodeResult<Stream> OpenWrite(VirtualPath path);
    /// <summary>
    /// Deletes an existing file.
    /// </summary>
    /// <param name="path">Path of file to delete.</param>
    /// <returns>Value indicating whether file was deleted.</returns>
    ExtendedErrorCode DeleteFile(VirtualPath path);
    /// <summary>
    /// Moves or renames a file.
    /// </summary>
    /// <param name="fileToMove">Full path of the file to move.</param>
    /// <param name="newFileName">New name and path of the file.</param>
    ExtendedErrorCode MoveFile(VirtualPath fileToMove, VirtualPath newFileName);

    /// <summary>
    /// Create a new directory.
    /// </summary>
    /// <param name="path">Path of directory to create.</param>
    ExtendedErrorCode CreateDirectory(VirtualPath path);
    /// <summary>
    /// Removes an existing directory.
    /// </summary>
    /// <param name="path">Path of directory to remove.</param>
    /// <returns>Value indicating whether directory was removed.</returns>
    ExtendedErrorCode RemoveDirectory(VirtualPath path);
}
