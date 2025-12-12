using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace MooParser;

[StructLayout(LayoutKind.Sequential, Size = 4)]
public readonly struct ChunkId : IEquatable<ChunkId>
{
    private readonly uint rawValue;

    public ChunkId(ReadOnlySpan<byte> value)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(value.Length, 4);
        this = MemoryMarshal.AsRef<ChunkId>(value);
    }

    public static readonly ChunkId Moo = new("MOO "u8);
    public static readonly ChunkId Meta = new("META"u8);
    public static readonly ChunkId Test = new("TEST"u8);
    public static readonly ChunkId Name = new("NAME"u8);
    public static readonly ChunkId Byts = new("BYTS"u8);
    public static readonly ChunkId Init = new("INIT"u8);
    public static readonly ChunkId Fina = new("FINA"u8);
    public static readonly ChunkId Regs = new("REGS"u8);
    public static readonly ChunkId Rmsk = new("RMSK"u8);
    public static readonly ChunkId Rg32 = new("RG32"u8);
    public static readonly ChunkId Rm32 = new("RM32"u8);
    public static readonly ChunkId Ram = new("RAM "u8);
    public static readonly ChunkId Queu = new("QUEU"u8);
    public static readonly ChunkId Ea32 = new("EA32"u8);

    public static bool operator ==(ChunkId left, ChunkId right) => left.Equals(right);
    public static bool operator !=(ChunkId left, ChunkId right) => !(left == right);

    public bool Equals(ChunkId other) => this.rawValue == other.rawValue;
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is ChunkId other && this.Equals(other);
    public override int GetHashCode() => this.rawValue.GetHashCode();
    public override string ToString() => Encoding.ASCII.GetString(MemoryMarshal.AsBytes(new ReadOnlySpan<uint>(in this.rawValue)));
}
