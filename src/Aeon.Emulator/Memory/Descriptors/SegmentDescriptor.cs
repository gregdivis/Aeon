using System.Runtime.InteropServices;
using Aeon.Emulator.Memory;

namespace Aeon.Emulator
{
    /// <summary>
    /// Contains information about a protected mode segment.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SegmentDescriptor
    {
        /// <summary>
        /// Big word mode in Attributes2.
        /// </summary>
        public const uint BigMode = 1 << 6;
        /// <summary>
        /// 4KB granularity mode in Attributes2.
        /// </summary>
        public const uint Granularity = 1 << 7;
        /// <summary>
        /// Indicates whether the segement is a code segment (1) or a data segment (0).
        /// </summary>
        public const uint CodeData = 1 << 3;
        /// <summary>
        /// For a code segment: 0 = XR, 1 = R. For a data segment: 0 = R, 1 = RW.
        /// </summary>
        public const uint ReadWrite = 1 << 1;
        /// <summary>
        /// Present flag in Attributes1.
        /// </summary>
        public const uint Present = 1 << 7;

        private ushort limit1;
        private ushort base1;
        private byte base2;
        private byte attributes1;
        private byte attributes2;
        private byte base3;

        /// <summary>
        /// Casts a segment descriptor to a descriptor.
        /// </summary>
        /// <param name="descriptor">Segment descriptor to cast.</param>
        /// <returns>Resulting descriptor.</returns>
        public static implicit operator Descriptor(SegmentDescriptor descriptor)
        {
            unsafe
            {
                return *(Descriptor*)&descriptor;
            }
        }

        /// <summary>
        /// Gets the physical base address of the segment.
        /// </summary>
        public uint Base => (uint)this.base1 | ((uint)this.base2 << 16) | ((uint)this.base3 << 24);
        /// <summary>
        /// Gets the size of the segment.
        /// </summary>
        public uint Limit => (uint)this.limit1 | (((uint)this.attributes2 & 0x0Fu) << 16);
        /// <summary>
        /// Gets attribute byte 1 of the descriptor.
        /// </summary>
        public byte Attributes1 => attributes1;
        /// <summary>
        /// Gets attribute byte 2 of the descriptor.
        /// </summary>
        public byte Attributes2 => attributes2;
        /// <summary>
        /// Gets the privilege level of the descriptor.
        /// </summary>
        public uint PrivilegeLevel => (uint)(this.attributes1 >> 5) & 0x3u;
        /// <summary>
        /// Gets the limit of the descriptor in bytes.
        /// </summary>
        public uint ByteLimit
        {
            get
            {
                if ((this.attributes2 & Granularity) == 0)
                    return this.Limit;
                else
                    return (this.Limit << 12) | 0xFFFu;
            }
        }
        /// <summary>
        /// Gets a value indicating whether the descriptor descrbies a code segment.
        /// </summary>
        public bool IsCodeSegment => (this.Attributes1 & CodeData) != 0;
        /// <summary>
        /// Gets a value indicating whether the segment can be read.
        /// </summary>
        public bool CanRead
        {
            get
            {
                if (this.IsCodeSegment)
                    return (this.Attributes1 & ReadWrite) != 0;
                else
                    return true;
            }
        }
        /// <summary>
        /// Gets a value indicating whether the segment can be written to.
        /// </summary>
        public bool CanWrite
        {
            get
            {
                if (!this.IsCodeSegment)
                    return (this.Attributes1 & ReadWrite) != 0;
                else
                    return false;
            }
        }
        /// <summary>
        /// Gets a value indicating whether the segment is present.
        /// </summary>
        public bool IsPresent => (this.attributes1 & Present) != 0;
    }
}
