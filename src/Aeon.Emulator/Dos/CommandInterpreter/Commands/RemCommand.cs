using System;
using System.Collections.Generic;
using System.Text;

namespace Aeon.Emulator.CommandInterpreter
{
    public sealed class RemCommand : CommandStatement
    {
        public RemCommand(string comment)
        {
            this.Comment = comment;
        }

        public string Comment { get; }

        internal override CommandResult Run(CommandProcessor processor) => CommandResult.Continue;
    }
}
