using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Aeon.Emulator;

namespace Aeon.DiskImages.Archives
{
    public sealed class ArchiveFile : IDisposable
    {
        private readonly Stream stream;
        private readonly ArchiveItem[] items;
        private readonly long dataStart;
        private bool disposed;

        public ArchiveFile(Stream stream)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));

            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                var id = new Guid(reader.ReadBytes(16));
                int version = reader.ReadInt32();
                this.items = new ArchiveItem[reader.ReadInt32()];
                for (int i = 0; i < items.Length; i++)
                {
                    var name = reader.ReadString();
                    var attributes = (VirtualFileAttributes)reader.ReadUInt16();
                    var writeTime = new DateTime(reader.ReadInt64(), DateTimeKind.Utc).ToLocalTime();
                    long size = reader.ReadInt64();
                    long rawSize = reader.ReadInt64();
                    long offset = reader.ReadInt64();
                    this.items[i] = new ArchiveItem(name, attributes, writeTime, offset, size, rawSize);
                }
            }

            this.dataStart = stream.Position;
        }

        public Stream OpenItem(string name)
        {
            var item = this.GetItem(name);
            if (item != null)
                return this.OpenItem(item);
            else
                return null;
        }

        public ArchiveItem GetItem(string name)
        {
            foreach (var item in this.items)
            {
                if (item.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return item;
            }

            return null;
        }
        public bool FileExists(string name)
        {
            var item = this.GetItem(name);
            if (item == null)
                return false;

            return !item.Attributes.HasFlag(VirtualFileAttributes.Directory);
        }
        public bool DirectoryExists(string name)
        {
            var dirName = name?.TrimEnd('\\');
            if (string.IsNullOrEmpty(dirName))
                return true;

            var fullDirName = dirName + '\\';
            if (this.items.Any(i => i.Name.StartsWith(fullDirName, StringComparison.OrdinalIgnoreCase)))
                return true;

            var item = this.GetItem(dirName);
            if (item != null)
                return item.Attributes.HasFlag(VirtualFileAttributes.Directory);
            else
                return false;
        }
        public IEnumerable<ArchiveItem> GetItems(string path)
        {
            var dir = path?.Trim('\\');
            if (string.IsNullOrEmpty(dir))
                return this.items.Where(i => !i.Name.Contains('\\'));

            var fullDir = dir + '\\';
            return this.items.Where(isInDir);

            bool isInDir(ArchiveItem item)
            {
                if (!item.Name.StartsWith(fullDir, StringComparison.OrdinalIgnoreCase))
                    return false;

                return item.Name.IndexOf('\\', fullDir.Length) < 0;
            }
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.stream.Dispose();
                this.disposed = true;
            }
        }

        private Stream OpenItem(ArchiveItem item)
        {
            if (item.DataOffset == -1)
                return Stream.Null;

            int type;

            lock (this.stream)
            {
                this.stream.Position = this.dataStart + item.DataOffset;
                type = this.stream.ReadByte();
            }

            var itemStream = new ItemStream(this.stream, this.dataStart + item.DataOffset + 1, item.RawSize - 1);
            if (type == 0)
                return itemStream;
            else if (type == 1)
                return new ChunkedStreamReader(itemStream, item.Size);
            else
                throw new InvalidDataException();
        }

        private sealed class ItemStream : Stream
        {
            private readonly Stream baseStream;
            private readonly long startOffset;

            public ItemStream(Stream baseStream, long start, long length)
            {
                this.baseStream = baseStream;
                this.startOffset = start;
                this.Length = length;
            }

            public override bool CanRead => true;
            public override bool CanSeek => true;
            public override bool CanWrite => false;
            public override long Length { get; }
            public override long Position { get; set; }

            public override int Read(Span<byte> buffer)
            {
                int maxBytes = (int)Math.Min(buffer.Length, this.Length - this.Position);
                if (maxBytes < 1)
                    return 0;

                lock (this.baseStream)
                {
                    this.baseStream.Position = this.startOffset + this.Position;
                    this.baseStream.Read(buffer.Slice(0, maxBytes));
                }

                this.Position += maxBytes;
                return maxBytes;
            }
            public override int Read(byte[] buffer, int offset, int count) => this.Read(buffer.AsSpan(offset, count));
            public override int ReadByte()
            {
                if (this.Position < this.Length)
                {
                    lock (this.baseStream)
                    {
                        this.baseStream.Position = this.startOffset + this.Position;
                        this.Position++;
                        return this.baseStream.ReadByte();
                    }
                }

                return -1;
            }
            public override long Seek(long offset, SeekOrigin origin)
            {
                return this.Position = origin switch
                {
                    SeekOrigin.Begin => offset,
                    SeekOrigin.Current => this.Position + offset,
                    SeekOrigin.End => this.Length + offset,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
            public override void Flush()
            {
            }
        }
    }
}
