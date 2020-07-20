using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aeon.Emulator.Dos.Programs;

namespace Aeon.Emulator.Dos.VirtualFileSystem
{
    /// <summary>
    /// Represents an emulated drive.
    /// </summary>
    public sealed class VirtualDrive
    {
        private string labelOverride;
        private long? freeSpaceOverride;
        /// <summary>
        /// Empty VirtualFileInfo array.
        /// </summary>
        private static readonly VirtualFileInfo[] EmptyFileList = new VirtualFileInfo[0];
        /// <summary>
        /// Command interpreter file info.
        /// </summary>
        private static readonly VirtualFileInfo CommandFileInfo = new VirtualFileInfo("COMMAND.COM", VirtualFileAttributes.Default, new DateTime(1995, 1, 1), CommandInterpreterStream.StreamLength);
        /// <summary>
        /// Array containing only the command interpreter.
        /// </summary>
        private static readonly VirtualFileInfo[] CommandFileList = new[] { CommandFileInfo };
        /// <summary>
        /// Wildcards for file filters.
        /// </summary>
        private static readonly char[] WildcardList = new[] { '*', '?', '.' };

        internal VirtualDrive()
        {
        }

        /// <summary>
        /// Gets or sets the type of drive to emulate.
        /// </summary>
        public DriveType DriveType { get; set; }
        /// <summary>
        /// Gets or sets the drive mapping source.
        /// </summary>
        public IMappedDrive Mapping { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the drive has a command interpreter installed.
        /// </summary>
        public bool HasCommandInterpreter { get; set; }
        /// <summary>
        /// Gets detailed information about a magnetic drive.
        /// </summary>
        /// <remarks>
        /// This may be null if the drive is optical.
        /// </remarks>
        public IMagneticDrive MagneticDriveInfo
        {
            get
            {
                if (this.Mapping is IMagneticDrive info)
                    return info;

                if (this.DriveType == DriveType.Floppy35)
                    return DefaultMagneticDriveInfo.Floppy35;
                else if (this.DriveType == DriveType.Floppy525)
                    return DefaultMagneticDriveInfo.Fixed;
                else if (this.DriveType == DriveType.Fixed)
                    return DefaultMagneticDriveInfo.Fixed;
                else
                    return null;
            }
        }
        /// <summary>
        /// Gets or sets the volume label for the drive.
        /// </summary>
        public string VolumeLabel
        {
            get => this.labelOverride ?? this.Mapping?.VolumeLabel;
            set => this.labelOverride = value;
        }
        /// <summary>
        /// Gets or sets the free space for the drive.
        /// </summary>
        public long FreeSpace
        {
            get => this.freeSpaceOverride ?? this.Mapping?.FreeSpace ?? 0;
            set => this.freeSpaceOverride = value;
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
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (this.HasCommandInterpreter && path.GetRelativePart() == ComFile.CommandPath)
            {
                if (fileMode == FileMode.Open && fileAccess == FileAccess.Read)
                    return new CommandInterpreterStream();
                else
                    return ExtendedErrorCode.FileNotFound;
            }

            var drive = this.Mapping;
            if (drive == null)
                return ExtendedErrorCode.InvalidDrive;

            if (fileMode == FileMode.Open && fileAccess == FileAccess.Read)
            {
                return drive.OpenRead(path);
            }
            else
            {
                if (!(this.Mapping is IWritableMappedDrive writableDrive))
                    return ExtendedErrorCode.AccessDenied;

                if (fileMode == FileMode.Open || fileMode == FileMode.OpenOrCreate)
                    return writableDrive.OpenWrite(path);
                else if (fileMode == FileMode.Create)
                    return writableDrive.CreateFile(path);
                else
                    throw new NotImplementedException();
            }
        }
        /// <summary>
        /// Returns a list of all of the files in a path inside a virtual directory.
        /// </summary>
        /// <param name="path">Path inside virtual directory to get list of files from.</param>
        /// <returns>List of files in the specified path.</returns>
        public ErrorCodeResult<IEnumerable<VirtualFileInfo>> GetDirectory(VirtualPath path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            var drive = this.Mapping;
            if (drive == null)
            {
                if (this.HasCommandInterpreter)
                    return Array.AsReadOnly(CommandFileList);
                else
                    return EmptyFileList;
            }

            var filter = path.LastElement;
            var contents = drive.GetDirectory(path.Elements.Count > 0 ? path.GetParent() : path);
            if (contents.Result == null)
                return contents;

            if (this.HasCommandInterpreter && path.GetParent().Path == string.Empty)
                contents = new ErrorCodeResult<IEnumerable<VirtualFileInfo>>(contents.Result.Union(CommandFileList));

            return new ErrorCodeResult<IEnumerable<VirtualFileInfo>>(VirtualDirectory.ApplyFilter(filter, contents.Result, f => f.Name));
        }
        /// <summary>
        /// Gets information about a file.
        /// </summary>
        /// <param name="path">Path to file in the file system.</param>
        /// <returns>Information about the file if it is found; otherwise null.</returns>
        public ErrorCodeResult<VirtualFileInfo> GetFileInfo(VirtualPath path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (this.HasCommandInterpreter && path == ComFile.CommandPath)
                return CommandFileInfo;

            var drive = this.Mapping;
            if (drive == null)
                return ExtendedErrorCode.InvalidDrive;

            if (path.Path == string.Empty)
                return new VirtualFileInfo(".", VirtualFileAttributes.Directory, DateTime.Now, 0);

            return drive.GetFileInfo(path);
        }
        /// <summary>
        /// Deletes a file.
        /// </summary>
        /// <param name="path">Path to file in the file system.</param>
        /// <returns>True if file was delete; false if file was not found.</returns>
        public ExtendedErrorCode DeleteFile(VirtualPath path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (!(this.Mapping is IWritableMappedDrive writableDrive))
                return ExtendedErrorCode.AccessDenied;

            return writableDrive.DeleteFile(path);
        }
        /// <summary>
        /// Moves or renames a file.
        /// </summary>
        /// <param name="fileToMove">Path of the file to move.</param>
        /// <param name="newFileName">New name and path of the file.</param>
        public ExtendedErrorCode MoveFile(VirtualPath fileToMove, VirtualPath newFileName)
        {
            if (fileToMove == null)
                throw new ArgumentNullException(nameof(fileToMove));
            if (newFileName == null)
                throw new ArgumentNullException(nameof(newFileName));

            if (!(this.Mapping is IWritableMappedDrive writableDrive))
                return ExtendedErrorCode.AccessDenied;

            return writableDrive.MoveFile(fileToMove, newFileName);
        }
        /// <summary>
        /// Create a new directory.
        /// </summary>
        /// <param name="path">Path of directory to create.</param>
        /// <returns>Value indicating whether directory was created.</returns>
        public ExtendedErrorCode CreateDirectory(VirtualPath path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (!(this.Mapping is IWritableMappedDrive writableDrive))
                return ExtendedErrorCode.AccessDenied;

            return writableDrive.CreateDirectory(path);
        }
        /// <summary>
        /// Removes an existing directory.
        /// </summary>
        /// <param name="path">Path of directory to remove.</param>
        /// <returns>Value indicating whether directory was removed.</returns>
        public ExtendedErrorCode RemoveDirectory(VirtualPath path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (!(this.Mapping is IWritableMappedDrive writableDrive))
                return ExtendedErrorCode.AccessDenied;

            return writableDrive.RemoveDirectory(path);
        }
    }
}
