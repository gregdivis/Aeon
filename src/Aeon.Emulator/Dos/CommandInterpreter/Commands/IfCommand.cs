using System;
using System.Collections.Generic;
using System.Text;

namespace Aeon.Emulator.CommandInterpreter
{
    public abstract class IfCommand : CommandStatement
    {
        protected IfCommand(bool not, CommandStatement command)
        {
            this.Not = not;
            this.Command = command;
        }

        public bool Not { get; }
        public CommandStatement Command { get; }
    }

    public sealed class IfErrorLevelCommand : IfCommand
    {
        public IfErrorLevelCommand(bool not, int errorLevel, CommandStatement command) : base(not, command)
        {
            this.ErrorLevel = errorLevel;
        }

        public int ErrorLevel { get; }

        internal override CommandResult Run(CommandProcessor processor) => processor.RunCommand(this);
    }

    public sealed class IfFileExistsCommand : IfCommand
    {
        public IfFileExistsCommand(bool not, string fileName, CommandStatement command) : base(not, command)
        {
            this.FileName = fileName;
        }

        public string FileName { get; }

        internal override CommandResult Run(CommandProcessor processor) => processor.RunCommand(this);
    }

    public sealed class IfEqualsCommand : IfCommand
    {
        public IfEqualsCommand(bool not, string value1, string value2, CommandStatement command) : base(not, command)
        {
            this.Value1 = value1;
            this.Value2 = value2;
        }

        public string Value1 { get; }
        public string Value2 { get; }

        internal override CommandResult Run(CommandProcessor processor) => processor.RunCommand(this);
    }
}
