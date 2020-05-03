using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Aeon.Emulator.CommandInterpreter
{
    public sealed class BatchFile
    {
        public BatchFile()
        {
        }
        public BatchFile(TextReader textReader)
        {
            if (textReader == null)
                throw new ArgumentNullException(nameof(textReader));
        }

        /// <summary>
        /// Represents a command in a batch file.
        /// </summary>
        private sealed class BatchCommand
        {
            /// <summary>
            /// Prevents a default instance of the <see cref="BatchCommand"/> class from being created.
            /// </summary>
            private BatchCommand()
            {
            }

            /// <summary>
            /// Gets the line number of the batch file command.
            /// </summary>
            public int Line { get; private set; }
            /// <summary>
            /// Gets the parsed batch file command.
            /// </summary>
            public Command Command { get; private set; }
            /// <summary>
            /// Gets a value indicating whether echo is enabled for the command.
            /// </summary>
            public bool IsEchoEnabled { get; private set; }

            /// <summary>
            /// Parses a line in a batch file.
            /// </summary>
            /// <param name="s">Line in batch file to parse.</param>
            /// <param name="lineNumber">Line number of the line in the batch file.</param>
            /// <returns>Parsed batch file command.</returns>
            public static BatchCommand Parse(string s, int lineNumber)
            {
                if (string.IsNullOrEmpty(s))
                    throw new ArgumentNullException(nameof(s));

                bool echo = true;

                if (s.StartsWith("@"))
                {
                    echo = false;
                    s = s.Substring(1);
                }

                var command = Parser.Parse(s);

                return new BatchCommand
                {
                    Line = lineNumber,
                    Command = command,
                    IsEchoEnabled = echo
                };
            }

            /// <summary>
            /// Returns a <see cref="System.String"/> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String"/> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                return string.Format("{0}: {1}{2}", this.Line, this.IsEchoEnabled ? "@" : "", this.Command);
            }
        }
    }
}
