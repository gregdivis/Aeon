namespace Aeon.Emulator;

/// <summary>
/// Specifies creation options for a <see cref="VirtualMachine"/> instance.
/// </summary>
public sealed class VirtualMachineInitializationOptions
{
    /// <summary>
    /// Gets or sets the size of emulated RAM in megabytes.
    /// </summary>
    public int PhysicalMemorySize
    {
        get;
        init
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 2);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, 2048);
            field = value;
        }
    } = 16;
    /// <summary>
    /// Gets or sets additional virtual devices to register.
    /// </summary>
    public IEnumerable<Func<VirtualMachine, IVirtualDevice>> AdditionalDevices
    {
        get;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    } = [];
}
