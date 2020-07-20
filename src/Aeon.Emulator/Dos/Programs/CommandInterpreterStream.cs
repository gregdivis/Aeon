using System;
using System.IO;

namespace Aeon.Emulator.Dos.Programs
{
    /// <summary>
    /// Totally unnecessary wrapper for a program data segment span.
    /// </summary>
    internal sealed partial class CommandInterpreterStream : Stream
    {
        private int position;

        public static int StreamLength => CommandInterpreter.Length;

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => CommandInterpreter.Length;
        public override long Position
        {
            get => this.position;
            set => this.position = (int)value;
        }

        public override int Read(Span<byte> buffer)
        {
            int count = Math.Min(buffer.Length, CommandInterpreter.Length - this.position);
            if (count > 0)
            {
                CommandInterpreter.Slice(this.position, count).CopyTo(buffer);
                this.position += count;
                return count;
            }
            else
            {
                return 0;
            }
        }
        public override int Read(byte[] buffer, int offset, int count) => this.Read(new Span<byte>(buffer, offset, count));
        public override int ReadByte()
        {
            if (this.position < CommandInterpreter.Length)
                return CommandInterpreter[this.position++];
            else
                return -1;
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.position = origin switch
            {
                SeekOrigin.Begin => (int)offset,
                SeekOrigin.Current => (int)offset + this.position,
                SeekOrigin.End => (int)offset + CommandInterpreter.Length,
                _ => throw new ArgumentException()
            };
        }
        public override void CopyTo(Stream destination, int bufferSize) => destination.Write(CommandInterpreter.Slice(this.position));
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override void Flush()
        {
        }
    }
}
