using System;
using System.Collections.Generic;
using System.Text;

namespace Aeon.Emulator.CommandInterpreter
{
    public sealed class SetCommand : CommandStatement
    {
        public SetCommand(string variable, string value)
        {
            this.Variable = variable;
            this.Value = value;
        }

        public string Variable { get; }
        public string Value { get; }

        internal override CommandResult Run(CommandProcessor processor) => processor.RunCommand(this);
    }
}
