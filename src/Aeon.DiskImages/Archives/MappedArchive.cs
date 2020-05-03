using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aeon.Emulator;
using Aeon.Emulator.Dos;
using Aeon.Emulator.Dos.VirtualFileSystem;

namespace Aeon.DiskImages.Archives
{
    public sealed class MappedArchive : IMappedDrive
    {
        public MappedArchive(DriveLetter drive, ArchiveFile archive)
        {
            this.Drive = drive;
            this.Archive = archive ?? throw new ArgumentNullException(nameof(archive));
        }

        public DriveLetter Drive { get; }
        public ArchiveFile Archive { get; }

        public string VolumeLabel => null;
        public long FreeSpace => 0;

        public ErrorCodeResult<IEnumerable<VirtualFileInfo>> GetDirectory(VirtualPath path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            return new ErrorCodeResult<IEnumerable<VirtualFileInfo>>(this.Archive.GetItems(GetArchivePath(path))
                .Select(Convert)
                .OrderBy(i => i.Name));
        }

        public ErrorCodeResult<VirtualFileInfo> GetFileInfo(VirtualPath path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            var item = this.Archive.GetItem(GetArchivePath(path));
            if (item != null)
                return Convert(item);
            else
                return ExtendedErrorCode.FileNotFound;
        }
        public ErrorCodeResult<Stream> OpenRead(VirtualPath path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            var s = this.Archive.OpenItem(this.GetArchivePath(path));
            if (s != null)
                return s;
            return ExtendedErrorCode.FileNotFound;
        }

        public void Dispose()
        {
        }

        private string GetArchivePath(VirtualPath path) => path.ChangeDrive(this.Drive).ToString();
        private static VirtualFileInfo Convert(ArchiveItem item) => new VirtualFileInfo(Path.GetFileName(item.Name).ToUpperInvariant(), item.Attributes, item.LastWriteTime, item.Size);
    }
}
