using System;

namespace Aeon.Emulator
{
    /// <summary>
    /// Provides information about an error in emulation.
    /// </summary>
    public sealed class ErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the ErrorEventArgs class.
        /// </summary>
        /// <param name="message">Message describing the error.</param>
        public ErrorEventArgs(string message) => this.Message = message;

        /// <summary>
        /// Gets a message describing the error.
        /// </summary>
        public string Message { get; }
    }
}
