namespace Aeon.Emulator.Decoding;

internal sealed class Mod3Exception : Exception
{
    public Mod3Exception()
        : base("Mod value was 3 on a memory-only operand.")
    {
    }
    public Mod3Exception(string message)
        : base(message)
    {
    }
    public Mod3Exception(string message, Exception inner)
        : base(message, inner)
    {
    }
}
