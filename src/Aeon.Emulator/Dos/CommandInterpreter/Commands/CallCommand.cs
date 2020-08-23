using System;

namespace Aeon.Emulator.CommandInterpreter
{
    public sealed class CallCommand : CommandStatement
    {
        public CallCommand(string target, string arguments)
        {
            if (string.IsNullOrWhiteSpace(target))
                throw new ArgumentNullException(nameof(target));

            this.Target = target;
            this.Arguments = arguments;
        }

        public string Target { get; }
        public string Arguments { get; set; }

        internal override CommandResult Run(CommandProcessor processor) => processor.RunCommand(this);
    }
}
