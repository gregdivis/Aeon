using System;
using System.IO;
using System.Text;
using System.Threading;

namespace Aeon.Emulator
{
    /// <summary>
    /// Represents the standard input and output streams for the emulated system.
    /// </summary>
    public sealed class VirtualConsole
    {
        public static readonly string NewLine = "\r\n";

        private readonly VirtualMachine vm;

        internal VirtualConsole(VirtualMachine vm) => this.vm = vm ?? throw new ArgumentNullException(nameof(vm));

        private Stream InStream => this.vm.Dos.StdIn;
        private Stream OutStream => this.vm.Dos.StdOut;
        private bool IsInputBlocking => this.InStream == this.vm.ConsoleIn;

        /// <summary>
        /// Returns the next byte.
        /// </summary>
        /// <returns>Next byte from input stream if available; otherwise -1.</returns>
        public int Read()
        {
            int value;
            while (true)
            {
                value = this.InStream.ReadByte();
                if (value >= 0 || !this.IsInputBlocking)
                    break;

                Thread.Sleep(50);
            }

            return value;
        }
        /// <summary>
        /// Returns the next whole line.
        /// </summary>
        /// <returns>Next line from input stream.</returns>
        public string ReadLine()
        {
            var sb = new StringBuilder();

            while (true)
            {
                int value = Read();
                if (value == -1 || value == '\n')
                    return sb.ToString();
                else if (value != '\r')
                    sb.Append((char)value);
            }
        }

        /// <summary>
        /// Writes a character.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public void Write(char value) => this.OutStream.WriteByte((byte)value);
        /// <summary>
        /// Writes a span of characters.
        /// </summary>
        /// <param name="value">Characters to write.</param>
        public void Write(ReadOnlySpan<char> value)
        {
            foreach (char c in value)
                this.Write(c);
        }
        /// <summary>
        /// Writes a string.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public void Write(string value)
        {
            if (!string.IsNullOrEmpty(value))
                this.Write(value.AsSpan());
        }

        /// <summary>
        /// Writes a string followed by a newline.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public void WriteLine(string value)
        {
            this.Write(value);
            this.Write(NewLine);
        }
        /// <summary>
        /// Writes a newline.
        /// </summary>
        public void WriteLine() => this.Write(NewLine);
        /// <summary>
        /// Writes a character followed by a newline.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public void WriteLine(char value)
        {
            this.Write(value);
            this.WriteLine();
        }
    }
}
