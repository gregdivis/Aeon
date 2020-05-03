using System;
using System.IO;

namespace Aeon.Emulator.Video
{
    internal sealed class ConsoleOutStream : Stream
    {
        private readonly TextConsole textConsole;

        public ConsoleOutStream(TextConsole textConsole) => this.textConsole = textConsole;

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => 0;
        public override long Position
        {
            get => 0;
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count)
        {
            for (int i = offset; i < count; i++)
                this.textConsole.Write(buffer[i]);
        }
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            foreach (var b in buffer)
                this.textConsole.Write(b);
        }
        public override void WriteByte(byte value) => this.textConsole.Write(value);
    }
}
