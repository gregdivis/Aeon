using System;
using System.Collections.Generic;
using System.Linq;

namespace Aeon.Emulator.CommandInterpreter
{
    public sealed class BatchFile
    {
        private readonly CommandStatement[] statements;

        public BatchFile(IEnumerable<CommandStatement> statements)
        {
            if (statements == null)
                throw new ArgumentNullException(nameof(statements));

            this.statements = statements.ToArray();
        }

        public ReadOnlySpan<CommandStatement> Statements => this.statements;
    }
}
