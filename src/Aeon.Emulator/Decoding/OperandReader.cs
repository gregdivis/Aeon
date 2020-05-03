using System;
using System.Runtime.InteropServices;

namespace Aeon.Emulator.Decoding
{
    internal ref struct OperandReader
    {
        private ReadOnlySpan<byte> data;

        public OperandReader(ReadOnlySpan<byte> data)
        {
            this.data = data;
            this.Position = 0;
        }

        public int Position { get; private set; }

        public byte ReadByte()
        {
            if (this.Position < 0 || this.Position >= data.Length)
                throw new InvalidOperationException();

            return this.data[this.Position++];
        }
        public sbyte ReadSByte() => (sbyte)ReadByte();
        public short ReadInt16() => (short)ReadUInt16();
        public ushort ReadUInt16()
        {
            if (MemoryMarshal.TryRead(this.data.Slice(this.Position, 2), out ushort value))
            {
                this.Position += 2;
                return value;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
        public uint ReadUInt32()
        {
            if (MemoryMarshal.TryRead(this.data.Slice(this.Position, 4), out ushort value))
            {
                this.Position += 4;
                return value;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}
