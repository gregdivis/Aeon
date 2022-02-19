namespace Aeon.Emulator
{
    /// <summary>
    /// Contains values used to initialize a new <see cref="VirtualMachine"/> instance.
    /// </summary>
    /// <param name="PhysicalMemory">The total amount of emulated RAM in megabytes.</param>
    /// <param name="EmsEnabled">Value indicating whether to emulate expanded memory management.</param>
    public sealed record class VirtualMachineStartupOptions(int PhysicalMemory = 16, bool EmsEnabled = true);
}
