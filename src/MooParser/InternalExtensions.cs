using System.Buffers.Binary;

namespace MooParser;

internal static class InternalExtensions
{
    extension(ReadOnlyMemory<byte> data)
    {
        public bool TryGetSubchunk(ChunkId type, out ReadOnlyMemory<byte> subchunk)
        {
            int offset = 0;
            var span = data.Span;
            while (offset < span.Length)
            {
                var id = new ChunkId(span.Slice(offset, 4));
                int length = BinaryPrimitives.ReadInt32LittleEndian(span.Slice(offset + 4, 4));
                if (id == type)
                {
                    subchunk = data.Slice(offset + 8, length);
                    return true;
                }

                offset += length + 8;
            }

            subchunk = default;
            return false;
        }
    }
}
