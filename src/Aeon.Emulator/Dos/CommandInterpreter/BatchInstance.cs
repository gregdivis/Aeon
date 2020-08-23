using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aeon.Emulator.CommandInterpreter
{
    internal sealed class BatchInstance
    {
        private readonly string[] arguments;

        public BatchInstance(BatchFile batch, string arguments)
        {
            this.Batch = batch;
            this.arguments = StatementParser.ParseArguments(arguments);
        }

        public BatchFile Batch { get; }
        public int CurrentLine { get; set; }
        public ReadOnlySpan<string> Arguments => this.arguments;

    }
}
