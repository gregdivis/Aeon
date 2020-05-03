using System;
using System.Threading;

namespace Aeon.Emulator.Sound.Blaster
{
    /// <summary>
    /// Stores bytes of data in a circular buffer.
    /// </summary>
    internal sealed class CircularBuffer
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the CircularBuffer class.
        /// </summary>
        /// <param name="capacity">Size of the buffer in bytes; the value must be a power of two.</param>
        public CircularBuffer(int capacity)
        {
            this.sizeMask = capacity - 1;
            this.data = new byte[capacity];
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets the size of the buffer in bytes.
        /// </summary>
        public int Capacity
        {
            get { return this.sizeMask + 1; }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Reads bytes from the buffer to an array and advances the read pointer.
        /// </summary>
        /// <param name="buffer">Array into which bytes are written.</param>
        /// <param name="offset">Offset into array to begin writing.</param>
        /// <param name="count">Number of bytes to read.</param>
        /// <returns>Number of bytes actually read.</returns>
        public int Read(byte[] buffer, int offset, int count)
        {
            int bufferBytes = this.bytesInBuffer;
            if(count > bufferBytes)
                count = bufferBytes;

            if(count > 0)
            {
                int readPos = this.readPosition;

                for(int i = 0; i < count; i++)
                {
                    buffer[offset + i] = this.data[readPos];
                    readPos = (readPos + 1) & this.sizeMask;
                }

                Interlocked.Add(ref this.bytesInBuffer, -count);
                Interlocked.Exchange(ref this.readPosition, readPos);
                this.readPosition = readPos;
            }

            return count;
        }
        /// <summary>
        /// Writes bytes from a location in memory to the buffer and advances the write pointer.
        /// </summary>
        /// <param name="source">Pointer to data to read.</param>
        /// <param name="count">Number of bytes to write.</param>
        /// <returns>Number of bytes actually written.</returns>
        public int Write(IntPtr source, int count)
        {
            int bytesAvailable = this.bytesInBuffer;
            int bytesFree = this.Capacity - bytesAvailable;

            if(count > bytesFree)
                count = bytesFree;

            if(count > 0)
            {
                unsafe
                {
                    byte* src = (byte*)source.ToPointer();
                    int writePos = this.writePosition;

                    for(int i = 0; i < count; i++)
                    {
                        this.data[writePos] = src[i];
                        writePos = (writePos + 1) & this.sizeMask;
                    }

                    Interlocked.Add(ref this.bytesInBuffer, count);
                    Interlocked.Exchange(ref this.writePosition, writePos);
                }
            }

            return count;
        }
        #endregion

        #region Private Fields
        private readonly byte[] data;
        private readonly int sizeMask;
        private int readPosition;
        private int writePosition;
        private int bytesInBuffer;
        #endregion
    }
}
