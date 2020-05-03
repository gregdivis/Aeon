using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aeon.DiskImages.Archives
{
    public sealed class ChunkedCompressor
    {
        private readonly List<Chunk> completedChunks = new List<Chunk>();
        private readonly Stream sourceStream;
        private readonly int chunkSize = 64 * 1024;
        private readonly double maxRatio = 0.9;
        private readonly Action<long> reportProgress;
        private long bytesCompleted;

        private ChunkedCompressor(Stream stream, Action<long> reportProgress)
        {
            this.sourceStream = stream;
            this.reportProgress = reportProgress;
        }

        public static void Compress(Stream source, Stream destination, Action<long> reportProgress = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));

            var compressor = new ChunkedCompressor(source, reportProgress);
            compressor.Compress(destination);
        }

        private void Compress(Stream output)
        {
            Parallel.For(0, (int)(this.sourceStream.Length / this.chunkSize) + 1, this.ReadChunk);

            this.completedChunks.Sort((c1, c2) => c1.Index.CompareTo(c2.Index));

            using var writer = new BinaryWriter(output, Encoding.UTF8, true);
            writer.Write(this.chunkSize);
            writer.Write(this.completedChunks.Count);

            uint offset = 0;
            foreach (var chunk in this.completedChunks)
            {
                writer.Write(offset);
                offset += (uint)chunk.Data.Length + 1u;
            }

            foreach (var chunk in this.completedChunks)
            {
                writer.Write((byte)chunk.Algorithm);
                writer.Write(chunk.Data);
            }
        }

        private void ReadChunk(int index)
        {
            var sourceBuffer = ArrayPool<byte>.Shared.Rent(this.chunkSize);
            try
            {
                int bytesRead;
                lock (this.sourceStream)
                {
                    this.sourceStream.Position = index * this.chunkSize;
                    bytesRead = this.sourceStream.Read(sourceBuffer, 0, sourceBuffer.Length);
                }

                var brotliData = CompressBrotli(sourceBuffer.AsSpan(0, bytesRead), out int brotliSize);
                try
                {
                    double ratio = (double)brotliSize / bytesRead;
                    if (brotliData != null && ratio <= this.maxRatio)
                    {
                        var data = brotliData.AsSpan(0, brotliSize).ToArray();
                        lock (this.completedChunks)
                        {
                            this.completedChunks.Add(new Chunk(index, CompressionAlgorithm.Brotli, data));
                        }
                    }
                    else
                    {
                        var data = sourceBuffer.AsSpan(0, bytesRead).ToArray();
                        lock (this.completedChunks)
                        {
                            this.completedChunks.Add(new Chunk(index, CompressionAlgorithm.Uncompressed, data));
                        }
                    }

                    this.reportProgress?.Invoke(Interlocked.Add(ref this.bytesCompleted, bytesRead));
                }
                finally
                {
                    if (brotliData != null)
                        ArrayPool<byte>.Shared.Return(brotliData);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(sourceBuffer);
            }
        }

        private static byte[] CompressBrotli(ReadOnlySpan<byte> chunk, out int compressedLength)
        {
            compressedLength = 0;
            int maxLength = BrotliEncoder.GetMaxCompressedLength(chunk.Length);
            var buffer = ArrayPool<byte>.Shared.Rent(maxLength);
            if (!BrotliEncoder.TryCompress(chunk, buffer, out int written, 11, 22))
            {
                ArrayPool<byte>.Shared.Return(buffer);
                return null;
            }

            compressedLength = written;
            return buffer;
        }

        private sealed class Chunk
        {
            public Chunk(int index, CompressionAlgorithm algorithm, byte[] data)
            {
                this.Index = index;
                this.Algorithm = algorithm;
                this.Data = data;
            }

            public int Index { get; }
            public CompressionAlgorithm Algorithm { get; }
            public byte[] Data { get; }

            public override string ToString() => this.Index.ToString();
        }
    }
}
