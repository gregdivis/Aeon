using System;

namespace Aeon.Emulator
{
    public sealed class MessageEventArgs : EventArgs
    {
        public MessageEventArgs(MessageLevel level, string message)
        {
            this.Level = level;
            this.Message = message;
        }

        public MessageLevel Level { get; }
        public string Message { get; }
    }
}
