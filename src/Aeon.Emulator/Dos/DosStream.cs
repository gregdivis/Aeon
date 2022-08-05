using System;
using System.IO;

namespace Aeon.Emulator.Dos
{
    internal sealed class DosStream
    {
        private readonly FileHandle handle;

        public DosStream(Stream stream, int ownerId)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            this.handle = new FileHandle(stream);
            this.OwnerId = ownerId;
        }
        private DosStream(FileHandle handle, int ownerId)
        {
            this.handle = handle;
            this.OwnerId = ownerId;
        }

        public bool CanRead => this.handle.Stream.CanRead;
        public bool CanSeek => this.handle.Stream.CanSeek;
        public bool CanWrite => this.handle.Stream.CanWrite;
        public void Flush() => this.handle.Stream.Flush();
        public long Length => this.handle.Stream.Length;
        public long Position
        {
            get => this.handle.Stream.Position;
            set => this.handle.Stream.Position = value;
        }
        public Stream BaseStream => this.handle.Stream;
        public int OwnerId { get; }
        public int SFTIndex
        {
            get => this.handle.SFTIndex;
            set => this.handle.SFTIndex = value;
        }
        public VirtualFileInfo? FileInfo
        {
            get => this.handle.FileInfo;
            set => this.handle.FileInfo = value;
        }
        public ushort HandleInfo
        {
            get
            {
                if (handle.Stream is IDeviceStream device)
                    return (ushort)((uint)device.DeviceInfo | 0x8080u);
                else if (this.FileInfo != null)
                    return (ushort)this.FileInfo.DeviceIndex;
                else
                    return 0;
            }
        }

        public void AddReference() => this.handle.AddReference();
        public DosStream CloneHandle(int newOwnerId)
        {
            this.handle.AddReference();
            return new DosStream(this.handle, newOwnerId);
        }
        public int Read(Span<byte> buffer) => this.handle.Stream.Read(buffer);
        public int ReadByte() => this.handle.Stream.ReadByte();
        public long Seek(long offset, SeekOrigin origin) => this.handle.Stream.Seek(offset, origin);
        public void SetLength(long value) => this.handle.Stream.SetLength(value);
        public void Write(ReadOnlySpan<byte> buffer) => this.handle.Stream.Write(buffer);
        public void WriteByte(byte value) => this.handle.Stream.WriteByte(value);
        public void Close() => this.handle.Close();
    }
}
