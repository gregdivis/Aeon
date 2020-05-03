using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Aeon.Emulator;
using Aeon.Emulator.Dos.VirtualFileSystem;

namespace Aeon.DiskImages.Archives
{
    public sealed class ArchiveBuilder : IDisposable
    {
        private static readonly Guid ArchiveId = new Guid("94E70904-924B-4525-A089-B556B091F34A");
        private const int ArchiveVersion = 1;
        private readonly List<ArchiveItem> items = new List<ArchiveItem>();
        private readonly List<Stream> itemSourceData = new List<Stream>();

        public int DataCount => this.itemSourceData.Count;

        public void AddFile(string sourceFileName, string targetFileName)
        {
            var info = new FileInfo(sourceFileName);
            int index = this.AddFileData(sourceFileName);
            this.items.Add(new ArchiveItem(targetFileName, MappedFolder.Convert(info.Attributes), info.LastWriteTimeUtc, index, info.Length));
        }
        public void AddFile(Stream source, string targetFileName)
        {
            int index = this.AddFileData(source);
            this.items.Add(new ArchiveItem(targetFileName, VirtualFileAttributes.Default, DateTime.UtcNow, index, source.Length));
        }

        public void Write(Stream stream, IArchiveBuilderProgress builderProgress = null)
        {
            using var tempStream = new FileStream(Path.GetTempFileName(), FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);
            var offsets = new List<long>();

            var itemMap = this.items
                .Where(i => i.Size > 0)
                .ToLookup(i => i.DataIndex);

            Action<long> streamProgress = null;
            if (builderProgress != null)
                streamProgress = builderProgress.ItemDataProcessed;

            int index = 0;
            foreach (var data in this.itemSourceData)
            {
                var items = itemMap[index];
                builderProgress?.ItemStart(index, string.Join(", ", items.Select(i => i.Name)), items.First().Size);
                offsets.Add(tempStream.Position);
                var (outputSize, compressed) = OptimizeStorage(data, tempStream, streamProgress);
                builderProgress?.ItemComplete(outputSize, compressed);
                foreach (var i in items)
                    i.RawSize = outputSize + 1;
                index++;
            }

            stream.Write(ArchiveId.ToByteArray(), 0, 16);
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                writer.Write(ArchiveVersion);
                writer.Write(this.items.Count);

                for (int i = 0; i < this.items.Count; i++)
                {
                    var item = this.items[i];
                    writer.Write(item.Name);
                    writer.Write((ushort)item.Attributes);
                    writer.Write(item.LastWriteTime.Ticks);
                    writer.Write(item.Size);
                    writer.Write(item.RawSize);
                    if (item.Size > 0)
                        writer.Write(offsets[item.DataIndex]);
                    else
                        writer.Write(-1L);
                }
            }

            tempStream.Position = 0;
            tempStream.CopyTo(stream);
        }

        private static (long outputSize, bool compressed) OptimizeStorage(Stream source, Stream destination, Action<long> reportProgress)
        {
            using var tempStream = CreateTempStream(source.Length);

            ChunkedCompressor.Compress(source, tempStream, reportProgress);

            if ((double)tempStream.Length / source.Length > 0.8)
            {
                destination.WriteByte(0);
                source.Position = 0;
                source.CopyTo(destination);
                return (source.Length, false);
            }
            else
            {
                destination.WriteByte(1);
                tempStream.Position = 0;
                tempStream.CopyTo(destination);
                return (tempStream.Length, true);
            }
        }

        private static Stream CreateTempStream(long length)
        {
            if (length <= 70 * 1024 * 1024)
                return new MemoryStream((int)length);
            else
                return new FileStream(Path.GetTempFileName(), FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);
        }

        private int AddFileData(string sourceFileName)
        {
            var srcStream = File.OpenRead(sourceFileName);
            if (srcStream.Length == 0)
            {
                srcStream.Dispose();
                return -1;
            }

            int i = 0;
            foreach (var item in this.itemSourceData)
            {
                if (AreEqual(item, srcStream))
                {
                    srcStream.Dispose();
                    return i;
                }

                i++;
            }

            this.itemSourceData.Add(srcStream);
            return this.itemSourceData.Count - 1;
        }
        private int AddFileData(Stream source)
        {
            this.itemSourceData.Add(source);
            return this.itemSourceData.Count - 1;
        }

        private static bool AreEqual(Stream a, Stream b)
        {
            if (a.Length != b.Length)
                return false;

            a.Position = 0;
            b.Position = 0;

            int value = a.ReadByte();
            while (value >= 0)
            {
                if (value != b.ReadByte())
                    return false;

                value = a.ReadByte();
            }

            return true;
        }

        public void Dispose()
        {
            foreach (var data in this.itemSourceData)
                data.Dispose();
        }

        internal sealed class ArchiveItem
        {
            public ArchiveItem(string name, VirtualFileAttributes attributes, DateTime lastWriteTime, int dataIndex, long size)
            {
                this.Name = name;
                this.DataIndex = dataIndex;
                this.Attributes = attributes;
                this.LastWriteTime = lastWriteTime;
                this.Size = size;
            }

            public string Name { get; }
            public int DataIndex { get; }
            public VirtualFileAttributes Attributes { get; }
            public DateTime LastWriteTime { get; }
            public long Size { get; }
            public long RawSize { get; set; }
        }
    }
}
