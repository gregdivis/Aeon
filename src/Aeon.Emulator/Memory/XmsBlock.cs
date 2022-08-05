using System;

namespace Aeon.Emulator.Memory
{
    /// <summary>
    /// Represents a block of XMS memory.
    /// </summary>
    internal readonly struct XmsBlock : IEquatable<XmsBlock>
    {
        public XmsBlock(int handle, uint offset, uint length, bool used)
        {
            this.Handle = handle;
            this.Offset = offset;
            this.Length = length;
            this.IsUsed = used;
        }

        /// <summary>
        /// Gets the handle which owns the block.
        /// </summary>
        public int Handle { get; }
        /// <summary>
        /// Gets the offset of the block from the XMS base address.
        /// </summary>
        public uint Offset { get; }
        /// <summary>
        /// Gets the length of the block in bytes.
        /// </summary>
        public uint Length { get; }
        /// <summary>
        /// Gets a value indicating whether the block is in use.
        /// </summary>
        public bool IsUsed { get; }

        public override string ToString()
        {
            if (this.IsUsed)
                return $"{this.Handle:X4}: {this.Offset:X8} to {this.Offset + this.Length:X8}";
            else
                return "Free";
        }
        public override bool Equals(object? obj) => obj is XmsBlock b && this.Equals(b);
        public override int GetHashCode() => this.Handle ^ (int)this.Offset ^ (int)this.Length;
        public bool Equals(XmsBlock other) => this.Handle == other.Handle && this.Offset == other.Offset && this.Length == other.Length && this.IsUsed == other.IsUsed;
        /// <summary>
        /// Allocates a block of memory from a free block.
        /// </summary>
        /// <param name="handle">Handle making the allocation.</param>
        /// <param name="length">Length of the requested block in bytes.</param>
        /// <returns>Array of blocks to replace this block.</returns>
        public XmsBlock[] Allocate(int handle, uint length)
        {
            if (this.IsUsed)
                throw new InvalidOperationException();
            if (length > this.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            if (length == this.Length)
                return new XmsBlock[1] { new XmsBlock(handle, this.Offset, length, true) };

            var blocks = new XmsBlock[2];

            blocks[0] = new XmsBlock(handle, this.Offset, length, true);
            blocks[1] = new XmsBlock(0, this.Offset + length, this.Length - length, false);

            return blocks;
        }
        /// <summary>
        /// Frees a used block of memory.
        /// </summary>
        /// <returns>Freed block to replace this block.</returns>
        public XmsBlock Free() => new XmsBlock(0, this.Offset, this.Length, false);
        /// <summary>
        /// Merges two contiguous unused blocks of memory.
        /// </summary>
        /// <param name="other">Other unused block to merge with.</param>
        /// <returns>Merged block of memory.</returns>
        public XmsBlock Join(XmsBlock other)
        {
            if (this.IsUsed | other.IsUsed)
                throw new InvalidOperationException();
            if (this.Offset + Length != other.Offset)
                throw new ArgumentException();

            return new XmsBlock(0, this.Offset, this.Length + other.Length, false);
        }
    }
}
