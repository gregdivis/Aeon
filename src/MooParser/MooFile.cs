using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace MooParser;

public sealed class MooFile
{
    private const uint MaxChunkLength = 1024 * 1024 * 1024;
    private readonly Stream stream;

    public MooFile(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!TryReadChunk(stream, out var type, out var moo) || type != ChunkId.Moo)
            throw new InvalidDataException("Expected MOO chunk at beginning of stream.");
        if (moo.Length < 12)
            throw new InvalidDataException($"MOO chunk had invalid length ({moo.Length}); expected 12.");

        this.MajorVersion = moo[0];
        if (this.MajorVersion != 1)
            throw new NotSupportedException("This reader only supports v1.x of the MOO format.");

        this.MinorVersion = moo[1];
        this.TestCount = BinaryPrimitives.ReadUInt32LittleEndian(moo.AsSpan(4, 4));
        this.CpuID = Encoding.ASCII.GetString(moo.AsSpan(8, 4));

        if (!TryReadChunk(stream, out type, out var meta) || type != ChunkId.Meta)
            throw new InvalidDataException("Missing META chunk.");

        this.Meta = new MetaChunk(meta);
        this.stream = stream;
    }

    public int MajorVersion { get; }
    public int MinorVersion { get; }
    public uint TestCount { get; }
    public string CpuID { get; }
    public MetaChunk Meta { get; }

    public IEnumerable<MooTest> EnumerateTests()
    {
        while (TryReadChunk(this.stream, out var type, out var data))
        {
            if (type == ChunkId.Test)
                yield return new MooTest(data);
        }
    }

    private static bool TryReadChunk(Stream stream, out ChunkId type, [MaybeNullWhen(false)] out byte[] data)
    {
        Span<byte> buffer = stackalloc byte[8];
        if (!tryReadExactly(stream, buffer))
        {
            type = default;
            data = null;
            return false;
        }

        type = new ChunkId(buffer[..4]);
        uint length = BinaryPrimitives.ReadUInt32LittleEndian(buffer[4..]);
        if (length >= MaxChunkLength)
            throw new InvalidDataException($"Chunk {type} had an unexpectedly large length of {length}.");

        data = new byte[length];
        stream.ReadExactly(data);
        return true;

        static bool tryReadExactly(Stream stream, Span<byte> buffer)
        {
            int bytesRead = 0;
            while (bytesRead < buffer.Length)
            {
                int res = stream.Read(buffer[bytesRead..]);
                bytesRead += res;
                if (res == 0 && bytesRead < buffer.Length)
                    return false;
            }

            return true;
        }
    }
}
