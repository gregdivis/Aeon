using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Aeon.Emulator.Dos.VirtualFileSystem
{
    /// <summary>
    /// Serves as the file system for DOS.
    /// </summary>
    public sealed class FileSystem
    {
        private const VirtualFileAttributes FindAttributeMask = VirtualFileAttributes.Hidden | VirtualFileAttributes.System | VirtualFileAttributes.VolumeLabel | VirtualFileAttributes.Directory;
        private readonly DriveList drives = new();
        private readonly VirtualPath[] currentDirectories = new VirtualPath[26];
        private DriveLetter currentDrive = DriveLetter.C;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystem"/> class.
        /// </summary>
        public FileSystem()
        {
            for (int i = 0; i < currentDirectories.Length; i++)
                currentDirectories[i] = VirtualPath.AbsoluteRoot;

            this.Drives[DriveLetter.C].DriveType = DriveType.Fixed;
        }

        /// <summary>
        /// Raised when the <see cref="CurrentDrive"/> property has changed.
        /// </summary>
        public event EventHandler? CurrentDriveChanged;

        /// <summary>
        /// Gets or sets the current drive and directory in the DOS session.
        /// </summary>
        public VirtualPath WorkingDirectory
        {
            get => ResolvePath(VirtualPath.RelativeCurrent);
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                if (value.PathType != VirtualPathType.Absolute)
                    throw new ArgumentException("Path must be absolute.");
                if (value.DriveLetter == null)
                    throw new ArgumentException("Path must specify drive.");

                this.currentDrive = (DriveLetter)value.DriveLetter;
                this.currentDirectories[this.currentDrive.Index] = value.ChangeDrive(null);
            }
        }
        public DriveLetter CurrentDrive
        {
            get => this.currentDrive;
            set
            {
                if (value != this.currentDrive)
                {
                    this.currentDrive = value;
                    this.OnCurrentDriveChanged(EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Gets the collection of emulated drives.
        /// </summary>
        public DriveList Drives => this.drives;
        /// <summary>
        /// Gets the path of the default command interpreter.
        /// </summary>
        public VirtualPath? CommandInterpreterPath
        {
            get
            {
                foreach (char c in "ABCDEFGHIJKLMNOPQRSTUVWXYZ")
                {
                    var drive = this.Drives[new DriveLetter(c)];
                    if (drive.HasCommandInterpreter)
                        return new VirtualPath($"{c}:\\COMMAND.COM");
                }

                return null;
            }
        }

        /// <summary>
        /// Returns an absolute path based on the current directory and drive.
        /// </summary>
        /// <param name="path">Path to make absolute.</param>
        /// <returns>Absolute path generated from the input path.</returns>
        public VirtualPath ResolvePath(VirtualPath path)
        {
            ArgumentNullException.ThrowIfNull(path);

            if (path.PathType == VirtualPathType.Absolute)
            {
                if (path.DriveLetter != null)
                    return path;
                else
                    return path.ChangeDrive(this.CurrentDrive);
            }
            else
            {
                if (path.DriveLetter != null)
                {
                    var newPath = this.currentDirectories[((DriveLetter)path.DriveLetter).Index] + path;
                    return newPath.ChangeDrive(path.DriveLetter);
                }
                else
                {
                    var newPath = this.currentDirectories[this.CurrentDrive.Index].ChangeDrive(this.CurrentDrive);
                    return newPath + path;
                }
            }
        }
        /// <summary>
        /// Gets the current directory for a drive in the DOS session.
        /// </summary>
        /// <param name="driveLetter">Drive letter whose directory will be returned.</param>
        /// <returns>Current directory for the specified drive.</returns>
        public VirtualPath GetCurrentDirectory(DriveLetter driveLetter) => this.currentDirectories[driveLetter.Index] ?? VirtualPath.AbsoluteRoot;
        /// <summary>
        /// Sets the current directory for a drive in the DOS session.
        /// </summary>
        /// <param name="path">New current directory.</param>
        public void ChangeDirectory(VirtualPath path)
        {
            ArgumentNullException.ThrowIfNull(path);

            path = ResolvePath(path);

            var drive = path.DriveLetter ?? currentDrive;
            this.currentDirectories[drive.Index] = path.ChangeDrive(null);
        }
        /// <summary>
        /// Opens an existing file or creates a new one.
        /// </summary>
        /// <param name="path">Relative path to file in the file system.</param>
        /// <param name="fileMode">Specifies how the file should be opened.</param>
        /// <param name="fileAccess">Specifies the desired access to the file.</param>
        /// <returns>Stream backed by the requested file.</returns>
        public ErrorCodeResult<Stream> OpenFile(VirtualPath path, FileMode fileMode, FileAccess fileAccess)
        {
            ArgumentNullException.ThrowIfNull(path);

            path = ResolvePath(path);

            return this.Drives[(DriveLetter)path.DriveLetter!].OpenFile(path, fileMode, fileAccess);
        }
        /// <summary>
        /// Opens an existing file or creates a new one.
        /// </summary>
        /// <param name="path">Relative path to file in the file system.</param>
        /// <param name="fileMode">Specifies how the file should be opened.</param>
        /// <param name="fileAccess">Specifies the desired access to the file.</param>
        /// <returns>Stream backed by the requested file.</returns>
        public ErrorCodeResult<Stream> OpenFile(string path, FileMode fileMode, FileAccess fileAccess)
        {
            ArgumentNullException.ThrowIfNull(path);

            return OpenFile(new VirtualPath(path), fileMode, fileAccess);
        }
        /// <summary>
        /// Returns a list of all of the files in a path inside a virtual directory.
        /// </summary>
        /// <param name="path">Path inside virtual directory to get list of files from.</param>
        /// <returns>List of files in the specified path.</returns>
        public ErrorCodeResult<IEnumerable<VirtualFileInfo>> GetDirectory(VirtualPath path)
        {
            ArgumentNullException.ThrowIfNull(path);

            path = ResolvePath(path);

            return this.Drives[(DriveLetter)path.DriveLetter!].GetDirectory(path);
        }
        /// <summary>
        /// Returns a list of all of the files in a path inside a virtual directory.
        /// </summary>
        /// <param name="path">Path inside virtual directory to get list of files from.</param>
        /// <returns>List of files in the specified path.</returns>
        public ErrorCodeResult<IEnumerable<VirtualFileInfo>> GetDirectory(string path)
        {
            ArgumentNullException.ThrowIfNull(path);

            return GetDirectory(new VirtualPath(path));
        }
        /// <summary>
        /// Returns a list of all of the files in a path inside a virtual directory.
        /// </summary>
        /// <param name="path">Path inside virtual directory to get list of files from.</param>
        /// <param name="includedAttributes">File attributes to include in the search.</param>
        /// <returns>List of files in the specified path.</returns>
        public ErrorCodeResult<IEnumerable<VirtualFileInfo>> GetDirectory(VirtualPath path, VirtualFileAttributes includedAttributes)
        {
            ArgumentNullException.ThrowIfNull(path);

            var labelEntries = Enumerable.Empty<VirtualFileInfo>();

            var data = this.GetDirectory(path);
            if (data.Result == null)
                return data;

            var results = data.Result;

            // This is hack. It should work as long as something isn't
            // counting on multiple volume labels on a drive.
            if (includedAttributes.HasFlag(VirtualFileAttributes.VolumeLabel))
            {
                path = ResolvePath(path);
                var label = this.Drives[(DriveLetter)path.DriveLetter!].VolumeLabel ?? string.Empty;
                results = new[] { new VirtualFileInfo(label, VirtualFileAttributes.VolumeLabel, DateTime.Now, 0) }.Concat(results);
            }

            return new ErrorCodeResult<IEnumerable<VirtualFileInfo>>(results.Where(f => (f.Attributes & FindAttributeMask & includedAttributes) == (f.Attributes & FindAttributeMask)));
        }
        /// <summary>
        /// Returns a list of all of the files in a path inside a virtual directory.
        /// </summary>
        /// <param name="path">Path inside virtual directory to get list of files from.</param>
        /// <param name="includedAttributes">File attributes to include in the search.</param>
        /// <returns>List of files in the specified path.</returns>
        public ErrorCodeResult<IEnumerable<VirtualFileInfo>> GetDirectory(string path, VirtualFileAttributes includedAttributes)
        {
            ArgumentNullException.ThrowIfNull(path);

            return this.GetDirectory(new VirtualPath(path), includedAttributes);
        }
        /// <summary>
        /// Gets information about a file.
        /// </summary>
        /// <param name="path">Path to file in the file system.</param>
        /// <returns>Information about the file if it is found; otherwise null.</returns>
        public ErrorCodeResult<VirtualFileInfo> GetFileInfo(VirtualPath path)
        {
            ArgumentNullException.ThrowIfNull(path);

            path = ResolvePath(path);

            var info = this.Drives[(DriveLetter)path.DriveLetter!].GetFileInfo(path);
            if (info.Result != null)
                info.Result.DeviceIndex = ((DriveLetter)path.DriveLetter).Index;

            return info;
        }
        /// <summary>
        /// Gets information about a file.
        /// </summary>
        /// <param name="path">Path to file in the file system.</param>
        /// <returns>Information about the file if it is found; otherwise null.</returns>
        public ErrorCodeResult<VirtualFileInfo> GetFileInfo(string path)
        {
            ArgumentNullException.ThrowIfNull(path);

            return GetFileInfo(new VirtualPath(path));
        }
        /// <summary>
        /// Deletes a file.
        /// </summary>
        /// <param name="path">Path to file in the file system.</param>
        /// <returns>True if file was delete; false if file was not found.</returns>
        public ExtendedErrorCode DeleteFile(VirtualPath path)
        {
            ArgumentNullException.ThrowIfNull(path);

            path = ResolvePath(path);

            return this.Drives[(DriveLetter)path.DriveLetter!].DeleteFile(path);
        }
        /// <summary>
        /// Deletes a file.
        /// </summary>
        /// <param name="path">Path to file in the file system.</param>
        /// <returns>True if file was delete; false if file was not found.</returns>
        public ExtendedErrorCode DeleteFile(string path)
        {
            ArgumentNullException.ThrowIfNull(path);

            return DeleteFile(new VirtualPath(path));
        }
        /// <summary>
        /// Returns a value indicating whether a file exists.
        /// </summary>
        /// <param name="path">Path of file to check for.</param>
        /// <returns>Value indicating whether file exists.</returns>
        public bool FileExists(VirtualPath path)
        {
            ArgumentNullException.ThrowIfNull(path);

            path = ResolvePath(path);

            if (path.Elements.Count == 0)
                return false;

            var fileInfo = GetFileInfo(path);
            return fileInfo.Result != null && (fileInfo.Result.Attributes & VirtualFileAttributes.Directory) == 0;
        }
        /// <summary>
        /// Returns a value indicating whether a file exists.
        /// </summary>
        /// <param name="path">Path of file to check for.</param>
        /// <returns>Value indicating whether file exists.</returns>
        public bool FileExists(string path)
        {
            ArgumentNullException.ThrowIfNull(path);

            return FileExists(new VirtualPath(path));
        }
        /// <summary>
        /// Returns a value indicating whether a directory exists.
        /// </summary>
        /// <param name="path">Path of directory to check for.</param>
        /// <returns>Value indicating whether directory exists.</returns>
        public bool DirectoryExists(VirtualPath path)
        {
            ArgumentNullException.ThrowIfNull(path);

            path = ResolvePath(path);

            if (path.Elements.Count == 0)
                return true;

            var fileInfo = GetFileInfo(path);
            return fileInfo.Result != null && (fileInfo.Result.Attributes & VirtualFileAttributes.Directory) != 0;
        }
        /// <summary>
        /// Returns a value indicating whether a directory exists.
        /// </summary>
        /// <param name="path">Path of directory to check for.</param>
        /// <returns>Value indicating whether directory exists.</returns>
        public bool DirectoryExists(string path)
        {
            ArgumentNullException.ThrowIfNull(path);

            return FileExists(new VirtualPath(path));
        }
        /// <summary>
        /// Creates a new directory.
        /// </summary>
        /// <param name="path">Path of directory to create.</param>
        /// <returns>Value indicating whether directory exists.</returns>
        public ExtendedErrorCode CreateDirectory(VirtualPath path)
        {
            ArgumentNullException.ThrowIfNull(path);

            path = ResolvePath(path);

            if (path.Elements.Count == 0)
                return ExtendedErrorCode.NoError;

            return this.Drives[(DriveLetter)path.DriveLetter!].CreateDirectory(path);
        }
        /// <summary>
        /// Creates a new directory.
        /// </summary>
        /// <param name="path">Path of directory to create.</param>
        /// <returns>Value indicating whether directory exists.</returns>
        public ExtendedErrorCode CreateDirectory(string path) => this.CreateDirectory(new VirtualPath(path));

        /// <summary>
        /// Raises the <see cref="E:CurrentDriveChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnCurrentDriveChanged(EventArgs e) => this.CurrentDriveChanged?.Invoke(this, e);
    }
}
