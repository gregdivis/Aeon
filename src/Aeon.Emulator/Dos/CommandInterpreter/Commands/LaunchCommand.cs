using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aeon.Emulator.CommandInterpreter
{
    public sealed class LaunchCommand : CommandStatement
    {
        public LaunchCommand(string target, string arguments)
        {
            this.Target = target;
            this.Arguments = arguments;
        }

        public string Target { get; }
        public string Arguments { get; }

        internal override CommandResult Run(CommandProcessor processor) => processor.RunCommand(this);
    }
}
