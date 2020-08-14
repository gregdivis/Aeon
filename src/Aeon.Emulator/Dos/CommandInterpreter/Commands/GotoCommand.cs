using System;
using System.Collections.Generic;
using System.Text;

namespace Aeon.Emulator.CommandInterpreter
{
    public sealed class GotoCommand : CommandStatement
    {
        public GotoCommand(string label)
        {
            this.Label = label;
        }

        public string Label { get; }

        internal override CommandResult Run(CommandProcessor processor) => processor.RunCommand(this);
    }
}
