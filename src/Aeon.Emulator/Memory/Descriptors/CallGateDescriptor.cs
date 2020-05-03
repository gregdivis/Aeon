using System.Runtime.InteropServices;

namespace Aeon.Emulator.Memory
{
    /// <summary>
    /// Represents a call gate descriptor.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CallGateDescriptor
    {
        private ushort offset1;
        private ushort selector;
        private byte countAttributes;
        private byte typeAttributes;
        private ushort offset2;

        /// <summary>
        /// Casts a call gate descriptor to a descriptor.
        /// </summary>
        /// <param name="descriptor">Call gate descriptor to cast.</param>
        /// <returns>Resulting descriptor.</returns>
        public static implicit operator Descriptor(CallGateDescriptor descriptor)
        {
            unsafe
            {
                return *(Descriptor*)&descriptor;
            }
        }

        /// <summary>
        /// Gets the segment offset.
        /// </summary>
        public uint Offset => this.offset1 | (uint)(this.offset2 << 16);
        /// <summary>
        /// Gets the selector value.
        /// </summary>
        public ushort Selector => this.selector;
        /// <summary>
        /// Gets the descriptor attributes.
        /// </summary>
        public byte Attributes => this.typeAttributes;
        /// <summary>
        /// Gets the privilege level of the descriptor.
        /// </summary>
        public uint PrivilegeLevel => ((uint)this.typeAttributes >> 5) & 0b11u;
        /// <summary>
        /// Gets the number of DWORD's to copy.
        /// </summary>
        public int DWordCount => this.countAttributes & 0b11111;
    }
}
