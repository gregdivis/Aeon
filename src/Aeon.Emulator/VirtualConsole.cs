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
        /// Writes a string.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public void Write(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                foreach (char c in value)
                    Write(c);
            }
        }
        /// <summary>
        /// Writes a boolean value.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public void Write(bool value) => Write(value.ToString());
        /// <summary>
        /// Writes an integer.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public void Write(int value) => Write(value.ToString());
        /// <summary>
        /// Writes a double.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public void Write(double value) => Write(value.ToString());
        /// <summary>
        /// Writes a single.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public void Write(float value) => Write(value.ToString());
        /// <summary>
        /// Writes a decimal.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public void Write(decimal value) => Write(value.ToString());
        /// <summary>
        /// Writes an unsigned integer.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public void Write(uint value) => Write(value.ToString());
        /// <summary>
        /// Writes an Int64.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public void Write(long value) => Write(value.ToString());
        /// <summary>
        /// Writes a UInt64.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public void Write(ulong value) => Write(value.ToString());
        /// <summary>
        /// Writes an object.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public void Write(object value)
        {
            if (value != null)
                Write(value.ToString());
        }
        /// <summary>
        /// Writes a formatted string.
        /// </summary>
        /// <param name="format">String formatting.</param>
        /// <param name="arg0">First argument.</param>
        public void Write(string format, object arg0)
        {
            if (format == null)
                throw new ArgumentNullException(nameof(format));

            Write(string.Format(format, arg0));
        }
        /// <summary>
        /// Writes a formatted string.
        /// </summary>
        /// <param name="format">String formatting.</param>
        /// <param name="arg0">First argument.</param>
        /// <param name="arg1">Second argument.</param>
        public void Write(string format, object arg0, object arg1)
        {
            if (format == null)
                throw new ArgumentNullException(nameof(format));

            Write(string.Format(format, arg0, arg1));
        }
        /// <summary>
        /// Writes a formatted string.
        /// </summary>
        /// <param name="format">String formatting.</param>
        /// <param name="arg0">First argument.</param>
        /// <param name="arg1">Second argument.</param>
        /// <param name="arg2">Third argument.</param>
        public void Write(string format, object arg0, object arg1, object arg2)
        {
            if (format == null)
                throw new ArgumentNullException(nameof(format));

            Write(string.Format(format, arg0, arg1, arg2));
        }
        /// <summary>
        /// Writes a formatted string.
        /// </summary>
        /// <param name="format">String formatting.</param>
        /// <param name="args">String format arguments.</param>
        public void Write(string format, params object[] args)
        {
            if (format == null)
                throw new ArgumentNullException(nameof(format));

            Write(string.Format(format, args));
        }

        /// <summary>
        /// Writes a string followed by a newline.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public void WriteLine(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                foreach (char c in value)
                    Write(c);
            }

            Write(NewLine);
        }
        /// <summary>
        /// Writes a newline.
        /// </summary>
        public void WriteLine() => Write(NewLine);
        /// <summary>
        /// Writes a boolean followed by a newline.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public void WriteLine(bool value)
        {
            Write(value);
            WriteLine();
        }
        /// <summary>
        /// Writes a character followed by a newline.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public void WriteLine(char value)
        {
            Write(value);
            WriteLine();
        }
        /// <summary>
        /// Writes an Int32 followed by a newline.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public void WriteLine(int value)
        {
            Write(value);
            WriteLine();
        }
        /// <summary>
        /// Writes a UInt32 followed by a newline.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public void WriteLine(uint value)
        {
            Write(value);
            WriteLine();
        }
        /// <summary>
        /// Writes an Int64 followed by a newline.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public void WriteLine(long value)
        {
            Write(value);
            WriteLine();
        }
        /// <summary>
        /// Writes a UInt64 followed by a newline.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public void WriteLine(ulong value)
        {
            Write(value);
            WriteLine();
        }
        /// <summary>
        /// Writes a double followed by a newline.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public void WriteLine(double value)
        {
            Write(value);
            WriteLine();
        }
        /// <summary>
        /// Writes a single followed by a newline.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public void WriteLine(float value)
        {
            Write(value);
            WriteLine();
        }
        /// <summary>
        /// Writes a decimal followed by a newline.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public void WriteLine(decimal value)
        {
            Write(value);
            WriteLine();
        }
        /// <summary>
        /// Writes an object followed by a newline.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public void WriteLine(object value)
        {
            Write(value);
            WriteLine();
        }
        /// <summary>
        /// Writes a formatted string followed by a newline.
        /// </summary>
        /// <param name="format">String formatting.</param>
        /// <param name="arg0">First argument.</param>
        public void WriteLine(string format, object arg0)
        {
            if (format == null)
                throw new ArgumentNullException(nameof(format));

            Write(string.Format(format, arg0));
            WriteLine();
        }
        /// <summary>
        /// Writes a formatted string followed by a newline.
        /// </summary>
        /// <param name="format">String formatting.</param>
        /// <param name="arg0">First argument.</param>
        /// <param name="arg1">Second argument.</param>
        public void WriteLine(string format, object arg0, object arg1)
        {
            if (format == null)
                throw new ArgumentNullException(nameof(format));

            Write(string.Format(format, arg0, arg1));
            WriteLine();
        }
        /// <summary>
        /// Writes a formatted string followed by a newline.
        /// </summary>
        /// <param name="format">String formatting.</param>
        /// <param name="arg0">First argument.</param>
        /// <param name="arg1">Second argument.</param>
        /// <param name="arg2">Third argument.</param>
        public void WriteLine(string format, object arg0, object arg1, object arg2)
        {
            if (format == null)
                throw new ArgumentNullException(nameof(format));

            Write(string.Format(format, arg0, arg1, arg2));
            WriteLine();
        }
        /// <summary>
        /// Writes a formatted string followed by a newline.
        /// </summary>
        /// <param name="format">String formatting.</param>
        /// <param name="args">String format arguments.</param>
        public void WriteLine(string format, params object[] args)
        {
            if (format == null)
                throw new ArgumentNullException(nameof(format));

            Write(string.Format(format, args));
            WriteLine();
        }
    }
}
