using System;
using System.Collections.Generic;
using System.Text;

namespace Aeon.Emulator.CommandInterpreter
{
    public sealed class TypeCommand : CommandStatement
    {
        public TypeCommand(string fileName)
        {
            this.FileName = fileName;
        }

        public string FileName { get; }

        internal override CommandResult Run(CommandProcessor processor) => processor.RunCommand(this);
    }
}
