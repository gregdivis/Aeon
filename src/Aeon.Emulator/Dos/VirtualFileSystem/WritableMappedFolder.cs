using System;
using System.IO;

namespace Aeon.Emulator.Dos.VirtualFileSystem
{
    /// <summary>
    /// Provides read/write access to a folder on the host system.
    /// </summary>
    public class WritableMappedFolder : MappedFolder, IWritableMappedDrive
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WritableMappedFolder"/> class.
        /// </summary>
        /// <param name="hostPath">Path of the mapped folder on the host system.</param>
        public WritableMappedFolder(string hostPath)
            : base(hostPath)
        {
        }

        public override long FreeSpace => 100 * 1024 * 1024;

        /// <summary>
        /// Creates a new file at the specified location, overwriting an existing file.
        /// </summary>
        /// <param name="path">Path of file to create.</param>
        /// <returns>Stream backed by the new file.</returns>
        public virtual ErrorCodeResult<Stream> CreateFile(VirtualPath path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            var fullPath = GetFullPath(path);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            return new FileStream(fullPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        }
        /// <summary>
        /// Opens an existing file for write access.
        /// </summary>
        /// <param name="path">Path of file to open.</param>
        /// <returns>Stream backed by the file.</returns>
        public virtual ErrorCodeResult<Stream> OpenWrite(VirtualPath path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            var fullPath = GetFullPath(path);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            //if (!Directory.Exists(Path.GetDirectoryName(fullPath)))
            //    return ExtendedErrorCode.PathNotFound;
            //else if (!File.Exists(fullPath))
            //    return ExtendedErrorCode.FileNotFound;

            return new FileStream(fullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        }
        /// <summary>
        /// Deletes an existing file.
        /// </summary>
        /// <param name="path">Path of file to delete.</param>
        /// <returns>Value indicating whether file was deleted.</returns>
        public virtual ExtendedErrorCode DeleteFile(VirtualPath path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            var fullPath = GetFullPath(path);

            if (Directory.Exists(Path.GetDirectoryName(fullPath)))
            {
                if (File.Exists(fullPath))
                {
                    try
                    {
                        File.Delete(fullPath);
                    }
                    catch
                    {
                        return ExtendedErrorCode.AccessDenied;
                    }

                    return ExtendedErrorCode.NoError;
                }
            }

            return ExtendedErrorCode.PathNotFound;
        }
        /// <summary>
        /// Moves or renames a file.
        /// </summary>
        /// <param name="fileToMove">Full path of the file to move.</param>
        /// <param name="newFileName">New name and path of the file.</param>
        public virtual ExtendedErrorCode MoveFile(VirtualPath fileToMove, VirtualPath newFileName)
        {
            if (fileToMove == null)
                throw new ArgumentNullException(nameof(fileToMove));
            if (newFileName == null)
                throw new ArgumentNullException(nameof(newFileName));

            var srcPath = GetFullPath(fileToMove);
            if (!File.Exists(srcPath))
                return ExtendedErrorCode.FileNotFound;

            var destPath = GetFullPath(newFileName);
            if (File.Exists(destPath))
                return ExtendedErrorCode.AccessDenied;

            File.Move(srcPath, destPath);
            return ExtendedErrorCode.NoError;
        }
        /// <summary>
        /// Create a new directory.
        /// </summary>
        /// <param name="path">Path of directory to create.</param>
        public virtual ExtendedErrorCode CreateDirectory(VirtualPath path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            var fullPath = GetFullPath(path);
            if (!Directory.Exists(Path.GetDirectoryName(fullPath)))
                return ExtendedErrorCode.PathNotFound;

            Directory.CreateDirectory(fullPath);
            return ExtendedErrorCode.NoError;
        }
        /// <summary>
        /// Removes an existing directory.
        /// </summary>
        /// <param name="path">Path of directory to remove.</param>
        /// <returns>Value indicating whether directory was removed.</returns>
        public virtual ExtendedErrorCode RemoveDirectory(VirtualPath path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            var fullPath = GetFullPath(path);

            if (Directory.Exists(fullPath))
            {
                try
                {
                    Directory.Delete(fullPath);
                    return ExtendedErrorCode.NoError;
                }
                catch
                {
                    return ExtendedErrorCode.AccessDenied;
                }
            }

            return ExtendedErrorCode.PathNotFound;
        }
    }
}
