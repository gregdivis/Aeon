using System;

namespace Aeon.Emulator.CommandInterpreter
{
    public sealed class CallCommand : CommandStatement
    {
        public CallCommand(string target)
        {
            if (string.IsNullOrWhiteSpace(target))
                throw new ArgumentNullException(nameof(target));

            this.Target = target;
        }

        public string Target { get; }

        internal override CommandResult Run(CommandProcessor processor) => processor.RunCommand(this);
    }
}
