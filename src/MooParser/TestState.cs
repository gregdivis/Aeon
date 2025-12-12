using System.Buffers.Binary;

namespace MooParser;

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
