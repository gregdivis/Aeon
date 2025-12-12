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

public sealed class MooTest
{
    internal MooTest(ReadOnlyMemory<byte> data)
    {
        this.Index = BinaryPrimitives.ReadUInt32LittleEndian(data.Span);
        data = data[4..];

        if (data.TryGetSubchunk(ChunkId.Name, out var nameChunk))
        {
            int nameLength = BinaryPrimitives.ReadInt32LittleEndian(nameChunk.Span);
            this.Name = Encoding.ASCII.GetString(nameChunk.Span.Slice(4, nameLength));
        }
        else
        {
            this.Name = string.Empty;
        }

        if (data.TryGetSubchunk(ChunkId.Byts, out var bytsChunk))
        {
            int bytsLength = BinaryPrimitives.ReadInt32LittleEndian(bytsChunk.Span);
            this.RawBytes = bytsChunk.Slice(4, bytsLength);
        }
        else
        {
            throw new InvalidDataException("Missing BYTS chunk.");
        }

        if (data.TryGetSubchunk(ChunkId.Init, out var initState))
            this.InitialState = new TestState(initState);
        else
            throw new InvalidDataException("Missing INIT chunk.");

        if (data.TryGetSubchunk(ChunkId.Fina, out var finalState))
            this.FinalState = new TestState(finalState);
        else
            throw new InvalidDataException("Missing FINA chunk.");
    }

    public uint Index { get; }
    public string Name { get; }
    public ReadOnlyMemory<byte> RawBytes { get; }
    public TestState InitialState { get; }
    public TestState FinalState { get; }
}

public sealed class TestState
{
    private readonly ReadOnlyMemory<byte> data;

    internal TestState(ReadOnlyMemory<byte> data) => this.data = data;

    public ReadOnlySpan<byte> InstructionQueue => this.data.TryGetSubchunk(ChunkId.Queu, out var sub) ? sub.Span : default;
    public EffectiveAddress32? EffectiveAddress32
    {
        get
        {
            if (this.data.TryGetSubchunk(ChunkId.Ea32, out var sub))
            {
                var span = sub.Span;
                return new EffectiveAddress32(
                    (SegmentRegister)span[0],
                    BinaryPrimitives.ReadUInt16LittleEndian(span[1..]),
                    BinaryPrimitives.ReadUInt32LittleEndian(span[3..]),
                    BinaryPrimitives.ReadUInt32LittleEndian(span[7..]),
                    BinaryPrimitives.ReadUInt32LittleEndian(span[11..]),
                    BinaryPrimitives.ReadUInt32LittleEndian(span[15..]),
                    BinaryPrimitives.ReadUInt32LittleEndian(span[19..])
                );
            }

            return null;
        }
    }

    public IEnumerable<KeyValuePair<RegMask, ushort>> GetRegisterValues() => this.GetRegistersInternal(ChunkId.Regs);
    public IEnumerable<KeyValuePair<RegMask, ushort>> GetRegisterMasks() => this.GetRegistersInternal(ChunkId.Rmsk);
    public IEnumerable<KeyValuePair<RegMask32, uint>> GetRegisterValues32() => this.GetRegisters32Internal(ChunkId.Rg32);
    public IEnumerable<KeyValuePair<RegMask32, uint>> GetRegisterMasks32() => this.GetRegisters32Internal(ChunkId.Rm32);
    public IEnumerable<KeyValuePair<uint, byte>> GetMemoryValues()
    {
        if (this.data.TryGetSubchunk(ChunkId.Ram, out var ramChunk))
        {
            int entryCount = BinaryPrimitives.ReadInt32LittleEndian(ramChunk.Span);
            int pos = 4;

            for (int i = 0; i < entryCount; i++)
            {
                uint address = BinaryPrimitives.ReadUInt32LittleEndian(ramChunk.Span[pos..]);
                pos += 4;
                byte value = ramChunk.Span[pos];
                pos++;
                yield return KeyValuePair.Create(address, value);
            }
        }
    }

    private IEnumerable<KeyValuePair<RegMask, ushort>> GetRegistersInternal(ChunkId type)
    {
        if (this.data.TryGetSubchunk(type, out var regsChunk))
        {
            var mask = (RegMask)BinaryPrimitives.ReadUInt16LittleEndian(regsChunk.Span);
            int pos = 2;
            foreach (var value in Enum.GetValues<RegMask>())
            {
                if (value == 0)
                    continue;

                if (mask.HasFlag(value))
                {
                    yield return KeyValuePair.Create(value, BinaryPrimitives.ReadUInt16LittleEndian(regsChunk.Span.Slice(pos, 2)));
                    pos += 2;
                }
            }
        }
    }

    private IEnumerable<KeyValuePair<RegMask32, uint>> GetRegisters32Internal(ChunkId type)
    {
        if (this.data.TryGetSubchunk(type, out var regsChunk))
        {
            var mask = (RegMask32)BinaryPrimitives.ReadUInt32LittleEndian(regsChunk.Span);
            int pos = 4;
            foreach (var value in Enum.GetValues<RegMask32>())
            {
                if (value == 0)
                    continue;

                if (mask.HasFlag(value))
                {
                    yield return KeyValuePair.Create(value, BinaryPrimitives.ReadUInt32LittleEndian(regsChunk.Span.Slice(pos, 4)));
                    pos += 4;
                }
            }
        }
    }
}

public sealed record EffectiveAddress32(SegmentRegister Register, ushort SegmentSelector, uint SegmentBaseAddress, uint SegmentLimit, uint Offset, uint LinearAddress, uint PhysicalAddress);

public enum SegmentRegister : byte
{
    CS,
    SS,
    DS,
    ES,
    FS,
    GS
}
