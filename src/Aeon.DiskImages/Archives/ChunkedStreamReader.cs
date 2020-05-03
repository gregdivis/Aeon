using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Aeon.DiskImages.Archives
{
    public sealed class ChunkedStreamReader : Stream
    {
        private readonly Stream baseStream;
        private readonly uint[] index;
        private readonly long startOffset;
        private readonly byte[] currentChunk;
        private byte[] decodeBuffer;
        private int currentIndex;
        private int chunkOffset;
        private bool chunkLoaded;

        public ChunkedStreamReader(Stream stream, long originalLength)
        {
            this.baseStream = stream;
            using var reader = new BinaryReader(stream, Encoding.UTF8, true);
            this.ChunkSize = reader.ReadInt32();
            this.index = new uint[reader.ReadInt32()];
            for (int i = 0; i < index.Length; i++)
                this.index[i] = reader.ReadUInt32();

            this.Length = originalLength;
            this.startOffset = stream.Position;
            this.currentChunk = new byte[this.ChunkSize];
        }

        public int ChunkSize { get; }
        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length { get; }
        public override long Position
        {
            get => (this.currentIndex * this.ChunkSize) + this.chunkOffset;
            set
            {
                int index = (int)(value / this.ChunkSize);
                int offset = (int)(value % this.ChunkSize);
                if (this.currentIndex != index)
                {
                    this.currentIndex = index;
                    this.chunkLoaded = false;
                }

                this.chunkOffset = offset;
            }
        }

        public override void Flush()
        {
        }
        public override int Read(Span<byte> buffer)
        {
            int bytesToRead = (int)Math.Min(buffer.Length, this.Length - this.Position);
            int bytesRead = 0;

            while (bytesRead < bytesToRead)
            {
                this.EnsureCurrentChunk();
                var src = this.currentChunk.AsSpan(this.chunkOffset, Math.Min(bytesToRead - bytesRead, this.currentChunk.Length - this.chunkOffset));
                var dest = buffer.Slice(bytesRead);
                src.CopyTo(dest);

                bytesRead += src.Length;
                this.chunkOffset += src.Length;
                if (this.chunkOffset >= this.ChunkSize)
                {
                    this.currentIndex++;
                    this.chunkOffset = 0;
                    this.chunkLoaded = false;
                }
            }

            return bytesRead;
        }
        public override int ReadByte()
        {
            Span<byte> buffer = stackalloc byte[1];
            return this.Read(buffer) > 0 ? buffer[0] : -1;
        }

        public override int Read(byte[] buffer, int offset, int count) => this.Read(buffer.AsSpan(offset, count));

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

        private void EnsureCurrentChunk()
        {
            if (!this.chunkLoaded)
            {
                this.baseStream.Position = this.startOffset + this.index[this.currentIndex];
                int rawChunkSize;
                if (this.currentIndex < this.index.Length - 1)
                    rawChunkSize = (int)(this.index[this.currentIndex + 1] - this.index[this.currentIndex] - 1);
                else
                    rawChunkSize = (int)(this.Length - this.index[this.currentIndex] - 1);

                var mode = (CompressionAlgorithm)this.baseStream.ReadByte();
                if (mode == CompressionAlgorithm.Uncompressed)
                {
                    this.baseStream.Read(this.currentChunk, 0, rawChunkSize);
                }
                else if (mode == CompressionAlgorithm.Brotli)
                {
                    if (rawChunkSize > (this.decodeBuffer?.Length ?? 0))
                        this.decodeBuffer = new byte[rawChunkSize];

                    this.baseStream.Read(this.decodeBuffer, 0, rawChunkSize);
                    BrotliDecoder.TryDecompress(this.decodeBuffer.AsSpan(0, rawChunkSize), this.currentChunk, out _);
                }
                else
                {
                    throw new InvalidDataException();
                }

                this.chunkLoaded = true;
            }
        }
    }
}
