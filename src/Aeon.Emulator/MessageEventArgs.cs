namespace Aeon.Emulator;

public sealed class MessageEventArgs(MessageLevel level, string message) : EventArgs
{
    public MessageLevel Level { get; } = level;
    public string Message { get; } = message;
}
