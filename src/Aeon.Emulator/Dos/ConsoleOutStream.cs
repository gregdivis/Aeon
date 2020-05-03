using System;
using System.IO;
using System.Text;
using Aeon.Emulator.Dos;
using Aeon.Emulator.Video;

namespace Aeon.Emulator
{
    /// <summary>
    /// Provides Stream-based access to the emulated text console device.
    /// </summary>
    public sealed class ConsoleOutStream : Stream, IDeviceStream
    {
        private readonly TextConsole console;

        internal ConsoleOutStream(TextConsole console) => this.console = console ?? throw new ArgumentNullException(nameof(console));

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => 0;
        public override long Position
        {
            get => 0;
            set => throw new NotSupportedException();
        }
        /// <summary>
        /// Gets information about the state of the device.
        /// </summary>
        public DosDeviceInfo DeviceInfo => DosDeviceInfo.ConsoleOutputDevice | DosDeviceInfo.SpecialDevice | DosDeviceInfo.NotEndOfFile;

        public override void Flush()
        {
        }
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override int ReadByte() => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => offset;
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset < 0 || offset >= buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0 || count > buffer.Length - offset)
                throw new ArgumentOutOfRangeException(nameof(count));

            string s = Encoding.ASCII.GetString(buffer, offset, count);
            this.console.Write(s);
        }
        /// <summary>
        /// Writes a string to the stream as ASCII bytes.
        /// </summary>
        /// <param name="s">String to write to the stream.</param>
        public void WriteString(string s)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));

            this.console.Write(s);
        }
        /// <summary>
        /// Writes a byte to the stream.
        /// </summary>
        /// <param name="value">Byte to write to the stream.</param>
        public override void WriteByte(byte value) => this.console.Write(value);
        /// <summary>
        /// Closes the stream.
        /// </summary>
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
