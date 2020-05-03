using System;

namespace Aeon.Emulator.Memory
{
    /// <summary>
    /// Provides read/write access to emulated physical memory.
    /// </summary>
    internal abstract class MemoryAccessor
    {
        /// <summary>
        /// Emulated physical memory.
        /// </summary>
        protected unsafe readonly byte* RawView;
        /// <summary>
        /// Emulated video device.
        /// </summary>
        protected Video.VideoHandler video;

        /// <summary>
        /// Initializes a new instance of the MemoryAccessor class.
        /// </summary>
        /// <param name="rawView">Emulated physical memory.</param>
        protected MemoryAccessor(IntPtr rawView)
        {
            unsafe
            {
                this.RawView = (byte*)rawView.ToPointer();
            }
        }

        public Video.VideoHandler VideoHandler
        {
            get => this.video;
            set => this.video = value;
        }

        /// <summary>
        /// Reads a byte from emulated memory.
        /// </summary>
        /// <param name="address">Physical address of byte to read.</param>
        /// <returns>Byte at the specified address.</returns>
        public abstract byte GetByte(uint address);
        /// <summary>
        /// Writes a byte to emulated memory.
        /// </summary>
        /// <param name="address">Physical address of byte to write.</param>
        /// <param name="value">Value to write to the specified address.</param>
        public abstract void SetByte(uint address, byte value);

        /// <summary>
        /// Reads an unsigned 16-bit integer from emulated memory.
        /// </summary>
        /// <param name="address">Physical address of unsigned 16-bit integer to read.</param>
        /// <returns>Unsigned 16-bit integer at the specified address.</returns>
        public abstract ushort GetUInt16(uint address);
        /// <summary>
        /// Writes an unsigned 16-bit integer to emulated memory.
        /// </summary>
        /// <param name="address">Physical address of unsigned 16-bit integer to write.</param>
        /// <param name="value">Value to write to the specified address.</param>
        public abstract void SetUInt16(uint address, ushort value);

        /// <summary>
        /// Reads an unsigned 32-bit integer from emulated memory.
        /// </summary>
        /// <param name="address">Physical address of unsigned 32-bit integer to read.</param>
        /// <returns>Unsigned 32-bit integer at the specified address.</returns>
        public abstract uint GetUInt32(uint address);
        /// <summary>
        /// Writes an unsigned 32-bit integer to emulated memory.
        /// </summary>
        /// <param name="address">Physical address of unsigned 32-bit integer to write.</param>
        /// <param name="value">Value to write to the specified address.</param>
        public abstract void SetUInt32(uint address, uint value);

        /// <summary>
        /// Reads an unsigned 64-bit integer from emulated memory.
        /// </summary>
        /// <param name="address">Physical address of unsigned 64-bit integer to read.</param>
        /// <returns>Unsigned 64-bit integer at the specified address.</returns>
        public abstract ulong GetUInt64(uint address);
        /// <summary>
        /// Writes an unsigned 64-bit integer to emulated memory.
        /// </summary>
        /// <param name="address">Physical address of unsigned 64-bit integer to write.</param>
        /// <param name="value">Value to write to the specified address.</param>
        public abstract void SetUInt64(uint address, ulong value);

        /// <summary>
        /// Returns a System.Single value read from an address in emulated memory.
        /// </summary>
        /// <param name="address">Address of value to read.</param>
        /// <returns>32-bit System.Single value read from the specified address.</returns>
        public abstract float GetReal32(uint address);
        /// <summary>
        /// Writes a System.Single value to an address in emulated memory.
        /// </summary>
        /// <param name="address">Address where value will be written.</param>
        /// <param name="value">32-bit System.Single value to write at the specified address.</param>
        public abstract void SetReal32(uint address, float value);

        /// <summary>
        /// Returns a System.Double value read from an address in emulated memory.
        /// </summary>
        /// <param name="address">Address of value to read.</param>
        /// <returns>64-bit System.Double value read from the specified address.</returns>
        public abstract double GetReal64(uint address);
        /// <summary>
        /// Writes a System.Double value to an address in emulated memory.
        /// </summary>
        /// <param name="address">Address where value will be written.</param>
        /// <param name="value">64-bit System.Double value to write at the specified address.</param>
        public abstract void SetReal64(uint address, double value);

        /// <summary>
        /// Returns a Real10 value read from an address in emulated memory.
        /// </summary>
        /// <param name="address">Address of value to read.</param>
        /// <returns>80-bit Real10 value read from the specified address.</returns>
        public abstract Real10 GetReal80(uint address);
        /// <summary>
        /// Writes a Real10 value to an address in emulated memory.
        /// </summary>
        /// <param name="address">Address where value will be written.</param>
        /// <param name="value">80-bit Real10 value to write at the specified address.</param>
        public abstract void SetReal80(uint address, Real10 value);

        /// <summary>
        /// Reads 16 bytes from emulated memory into a buffer.
        /// </summary>
        /// <param name="address">Address where bytes will be read from.</param>
        /// <param name="buffer">Buffer into which bytes will be written.</param>
        public abstract unsafe void FetchInstruction(uint address, byte* buffer);

        /// <summary>
        /// Returns a pointer to a block of memory, making sure it is paged in.
        /// </summary>
        /// <param name="address">Logical address of block.</param>
        /// <param name="size">Number of bytes in block of memory.</param>
        /// <returns>Pointer to block of memory.</returns>
        public abstract unsafe void* GetSafePointer(uint address, uint size);
    }
}
