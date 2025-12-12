using System.Buffers.Binary;
using System.Text;

namespace MooParser;

public sealed class MetaChunk
{
    public MetaChunk(ReadOnlySpan<byte> data)
    {
        this.MajorVersion = data[0];
        this.MinorVersion = data[1];
        this.CpuType = data[2];
        this.Opcode = BinaryPrimitives.ReadUInt32LittleEndian(data[3..7]);
        this.Mnemonic = Encoding.ASCII.GetString(data[7..15].TrimEnd((byte)' '));
        this.TestCount = BinaryPrimitives.ReadUInt32LittleEndian(data[15..19]);
        this.FileSeed = BinaryPrimitives.ReadUInt64LittleEndian(data[19..27]);
        this.CpuMode = data[27];
    }

    public int MajorVersion { get; }
    public int MinorVersion { get; }
    public byte CpuType { get; }
    public uint Opcode { get; }
    public string Mnemonic { get; }
    public uint TestCount { get; }
    public ulong FileSeed { get; }
    public byte CpuMode { get; }
}
