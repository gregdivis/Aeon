using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Aeon.Emulator;
using Aeon.Emulator.Dos;
using Aeon.Emulator.Dos.VirtualFileSystem;

namespace Aeon.DiskImages.Archives
{
    public sealed class DifferencingFolder : WritableMappedFolder
    {
        private readonly HashSet<VirtualPath> deletes;

        public DifferencingFolder(DriveLetter drive, ArchiveFile archive, string hostPath)
            : base(hostPath)
        {
            this.Drive = drive;
            this.Archive = archive ?? throw new ArgumentNullException(nameof(archive));
            this.deletes = this.ReadDeletes();
        }

        public DriveLetter Drive { get; }
        public ArchiveFile Archive { get; }

        public override ExtendedErrorCode CreateDirectory(VirtualPath path)
        {
            this.RemoveDelete(path);
            bool inArchive = this.Archive.DirectoryExists(GetArchivePath(path));

            if (inArchive)
                return ExtendedErrorCode.NoError;
            else
                return base.CreateDirectory(path);
        }
        public override ErrorCodeResult<Stream> CreateFile(VirtualPath path)
        {
            this.RemoveDelete(path);
            return base.CreateFile(path);
        }
        public override ExtendedErrorCode DeleteFile(VirtualPath path)
        {
            var result = base.DeleteFile(path);

            if (this.Archive.FileExists(GetArchivePath(path)))
            {
                this.AddDelete(path);
                return ExtendedErrorCode.NoError;
            }

            return result;
        }
        public override ExtendedErrorCode RemoveDirectory(VirtualPath path)
        {
            var result = base.RemoveDirectory(path);

            if (this.Archive.DirectoryExists(GetArchivePath(path)))
            {
                this.AddDelete(path);
                return ExtendedErrorCode.NoError;
            }

            return result;
        }
        public override ErrorCodeResult<Stream> OpenRead(VirtualPath path)
        {
            var d = this.IsDeleted(path);
            if (d != ExtendedErrorCode.NoError)
                return d;

            var fsResult = base.OpenRead(path);
            if (fsResult.Result != null)
                return fsResult;

            var archiveStream = this.Archive.OpenItem(this.GetArchivePath(path));
            if (archiveStream != null)
                return archiveStream;

            return fsResult;
        }
        public override ErrorCodeResult<VirtualFileInfo> GetFileInfo(VirtualPath path)
        {
            var d = this.IsDeleted(path);
            if (d != ExtendedErrorCode.NoError)
                return d;

            var fsResult = base.GetFileInfo(path);
            if (fsResult.Result != null)
                return fsResult;

            var archiveItem = this.Archive.GetItem(this.GetArchivePath(path));
            if (archiveItem != null)
                return Convert(archiveItem);

            return fsResult;
        }
        public override ErrorCodeResult<Stream> OpenWrite(VirtualPath path)
        {
            this.RemoveDelete(path);

            var fsStream = base.OpenWrite(path);
            if (fsStream.Result == null)
                return fsStream;

            using var archiveStream = this.Archive.OpenItem(this.GetArchivePath(path));
            if (archiveStream != null)
            {
                archiveStream.CopyTo(fsStream.Result);
                fsStream.Result.SetLength(fsStream.Result.Position);
                fsStream.Result.Position = 0;
            }

            return fsStream;
        }
        public override ExtendedErrorCode MoveFile(VirtualPath fileToMove, VirtualPath newFileName)
        {
            if (this.GetFileInfo(newFileName).Result != null)
                return ExtendedErrorCode.AccessDenied;

            var srcInfo = this.GetFileInfo(fileToMove);
            if (srcInfo.Result == null)
                return srcInfo.ErrorCode;

            using var srcStream = this.OpenRead(fileToMove).Result;
            if (srcStream == null)
                return ExtendedErrorCode.FileNotFound;

            using var destStream = this.CreateFile(newFileName).Result;
            if (destStream == null)
                return ExtendedErrorCode.PathNotFound;

            srcStream.CopyTo(destStream);

            this.AddDelete(fileToMove);
            this.RemoveDelete(newFileName);

            return ExtendedErrorCode.NoError;
        }
        public override ErrorCodeResult<IEnumerable<VirtualFileInfo>> GetDirectory(VirtualPath path)
        {
            var archiveItems = this.Archive.GetItems(this.GetArchivePath(path)).ToList();

            var fsResult = base.GetDirectory(path);
            if (fsResult.Result == null)
            {
                if (archiveItems.Count > 0)
                    return new ErrorCodeResult<IEnumerable<VirtualFileInfo>>(archiveItems.Select(Convert));

                return this.Archive.DirectoryExists(this.GetArchivePath(path)) ? new ErrorCodeResult<IEnumerable<VirtualFileInfo>>(Enumerable.Empty<VirtualFileInfo>()) : ExtendedErrorCode.PathNotFound;
            }

            if (archiveItems.Count == 0)
                return fsResult;

            var items = archiveItems.ToDictionary(i => i.Name, Convert, StringComparer.OrdinalIgnoreCase);
            foreach (var i in fsResult.Result)
                items[i.Name] = i;

            return new ErrorCodeResult<IEnumerable<VirtualFileInfo>>(items.Values.OrderBy(i => i.Name));
        }

        private ExtendedErrorCode IsDeleted(VirtualPath path)
        {
            var p = path.ChangeDrive(null);
            int n = 0;
            while (p.Elements.Count > 0)
            {
                if (this.deletes.Contains(p))
                    return n > 0 ? ExtendedErrorCode.PathNotFound : ExtendedErrorCode.FileNotFound;

                p = p.GetParent();
                n++;
            }

            return ExtendedErrorCode.NoError;
        }
        private void AddDelete(VirtualPath path)
        {
            if (this.deletes.Add(path.ChangeDrive(null)))
                this.WriteDeletes();
        }
        private void RemoveDelete(VirtualPath path)
        {
            bool modified = false;
            var p = path.ChangeDrive(null);
            while (p.Elements.Count > 0)
            {
                modified |= this.deletes.Remove(p);
                p = p.GetParent();
            }

            if (modified)
                this.WriteDeletes();
        }
        private void WriteDeletes()
        {
            try
            {
                var path = Path.Combine(this.HostPath, ".deletes");
                if (this.deletes.Count > 0)
                {
                    using var writer = new StreamWriter(path, false, Encoding.UTF8);
                    foreach (var item in this.deletes.OrderBy(i => i))
                        writer.Write(item.GetRelativePart().ToString());
                }
                else
                {
                    File.Delete(path);
                }
            }
            catch
            {
            }
        }
        private HashSet<VirtualPath> ReadDeletes()
        {
            return new HashSet<VirtualPath>(read());

            IEnumerable<VirtualPath> read()
            {
                var path = Path.Combine(this.HostPath, ".deletes");
                if (File.Exists(path))
                {
                    using var reader = File.OpenText(path);
                    string name;
                    while ((name = reader.ReadLine()) != null)
                    {
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            var p = VirtualPath.TryParse(name.Trim());
                            if (p != null)
                                yield return p;
                        }
                    }
                }
            }
        }
        private static VirtualFileInfo Convert(ArchiveItem item)
        {
            return new VirtualFileInfo(Path.GetFileName(item.Name).ToUpperInvariant(), item.Attributes, item.LastWriteTime, item.Size);
        }
        private static void EnsureRoot(string fullPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
        }
        private string GetArchivePath(VirtualPath path)
        {
            return path.ChangeDrive(this.Drive).ToString();
        }
    }
}
