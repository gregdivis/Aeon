using System;
using System.Collections.Generic;
using System.Text;

namespace Aeon.Emulator.CommandInterpreter
{
    public sealed class LabelStatement : CommandStatement
    {
        public LabelStatement(string name)
        {
            this.Name = name;
        }

        public string Name { get; }

        internal override CommandResult Run(CommandProcessor processor) => CommandResult.Continue;
    }
}
