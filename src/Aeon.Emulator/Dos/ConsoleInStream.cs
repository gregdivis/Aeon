using System;
using System.IO;
using Aeon.Emulator.Dos;
using Aeon.Emulator.Keyboard;

namespace Aeon.Emulator
{
    /// <summary>
    /// Provides stream-based access to the emulated keyboard device.
    /// </summary>
    public sealed class ConsoleInStream : Stream, IDeviceStream
    {
        private readonly KeyboardDevice keyboard;

        internal ConsoleInStream(KeyboardDevice keyboard) => this.keyboard = keyboard ?? throw new ArgumentNullException(nameof(keyboard));

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => 0;
        public override long Position
        {
            get => 0;
            set => throw new NotSupportedException();
        }
        /// <summary>
        /// Gets a value indicating whether there is at least one byte of data available to be read.
        /// </summary>
        public bool DataAvailable => this.keyboard.HasTypeAheadDataAvailable;
        /// <summary>
        /// Gets information about the state of the device.
        /// </summary>
        public DosDeviceInfo DeviceInfo
        {
            get
            {
                var info = DosDeviceInfo.ConsoleInputDevice | DosDeviceInfo.SpecialDevice;
                if (this.DataAvailable)
                    info |= DosDeviceInfo.NotEndOfFile;

                return info;
            }
        }

        public override void Flush()
        {
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset < 0 || offset >= buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0 || count > buffer.Length - offset)
                throw new ArgumentOutOfRangeException(nameof(count));

            for (int i = 0; i < count; i++)
            {
                ushort? value = this.keyboard.TryDequeueTypeAhead();
                if (value != null)
                    buffer[offset + i] = (byte)value;
                else
                    return i;
            }

            return count;
        }
        public override int ReadByte()
        {
            ushort? value = this.keyboard.TryDequeueTypeAhead();
            if (value != null)
                return ((ushort)value & 0xFF);
            else
                return -1;
        }
        public override long Seek(long offset, SeekOrigin origin) => offset;
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count)
        {
        }
        public override void WriteByte(byte value)
        {
        }
        public override void Close()
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
                base.Dispose(disposing);
        }
    }
}
