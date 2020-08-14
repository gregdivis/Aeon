using System;
using System.Collections.Generic;
using System.Text;

namespace Aeon.Emulator.CommandInterpreter
{
    public sealed class SetCurrentDirectoryCommand : CommandStatement
    {
        public SetCurrentDirectoryCommand(string path)
        {
            this.Path = path;
        }

        public string Path { get; }

        internal override CommandResult Run(CommandProcessor processor) => processor.RunCommand(this);
    }
}
