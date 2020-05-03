using System;

namespace Aeon.Emulator.RuntimeExceptions
{
    /// <summary>
    /// Exception raised to indicate when the CPU trap flag has been set.
    /// </summary>
    public sealed class EnableInstructionTrapException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnableInstructionTrapException"/> class.
        /// </summary>
        public EnableInstructionTrapException()
        {
        }
    }
}
