using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Text;

namespace MooParser;

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

        if (data.TryGetSubchunk(ChunkId.Cycl, out var cyclesChunk))
        {
            int count = BinaryPrimitives.ReadInt32LittleEndian(cyclesChunk.Span);
            if (count > 0)
            {
                var cycles = new CycleState[count];

                MemoryMarshal.Cast<byte, CycleState>(cyclesChunk.Span[4..]).CopyTo(cycles);
                if (!BitConverter.IsLittleEndian)
                {
                    for (int i = 0; i < cycles.Length; i++)
                    {
                        cycles[i].AddressLatch = BinaryPrimitives.ReverseEndianness(cycles[i].AddressLatch);
                        cycles[i].DataBus = BinaryPrimitives.ReverseEndianness(cycles[i].DataBus);
                    }
                }

                this.Cycles = [.. cycles];
            }
            else
            {
                this.Cycles = [];
            }
        }
        else
        {
            this.Cycles = [];
        }

        if (data.TryGetSubchunk(ChunkId.Excp, out var exception))
        {
            var span = exception.Span;
            this.Exception = new TestException(span[0], BinaryPrimitives.ReadUInt32LittleEndian(span[1..]));
        }

        if (data.TryGetSubchunk(ChunkId.Hash, out var hash))
            this.Hash = hash;
    }

    public uint Index { get; }
    public string Name { get; }
    public ReadOnlyMemory<byte> RawBytes { get; }
    public TestState InitialState { get; }
    public TestState FinalState { get; }
    public ImmutableArray<CycleState> Cycles { get; }
    public TestException? Exception { get; }
    public ReadOnlyMemory<byte> Hash { get; }
}
