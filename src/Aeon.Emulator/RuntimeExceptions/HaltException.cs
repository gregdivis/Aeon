namespace Aeon.Emulator.RuntimeExceptions;

public sealed class HaltException() : Exception("Emulated system was halted.")
{
}
