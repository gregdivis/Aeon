using System;
using Aeon.Emulator;

namespace Aeon.DiskImages.Archives
{
    public sealed class ArchiveItem
    {
        internal ArchiveItem(string name, VirtualFileAttributes attributes, DateTime lastWriteTime, long dataOffset, long size, long rawSize)
        {
            this.Name = name;
            this.DataOffset = dataOffset;
            this.Attributes = attributes;
            this.LastWriteTime = lastWriteTime;
            this.Size = size;
            this.RawSize = rawSize;
        }

        public string Name { get; }
        public long DataOffset { get; }
        public VirtualFileAttributes Attributes { get; }
        public DateTime LastWriteTime { get; }
        public long RawSize { get; }
        public long Size { get; }
    }
}
