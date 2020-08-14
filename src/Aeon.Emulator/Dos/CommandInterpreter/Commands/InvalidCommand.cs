using System;
using System.Collections.Generic;
using System.Text;

namespace Aeon.Emulator.CommandInterpreter
{
    public sealed class InvalidCommand : CommandStatement
    {
        public InvalidCommand(string error)
        {
            this.Error = error;
        }

        public string Error { get; }

        internal override CommandResult Run(CommandProcessor processor) => processor.RunCommand(this);
    }
}
