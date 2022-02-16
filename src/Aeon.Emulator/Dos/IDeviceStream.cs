namespace Aeon.Emulator.Dos
{
    /// <summary>
    /// Implemented by Streams to indicate emulation of a DOS device.
    /// </summary>
    public interface IDeviceStream
    {
        /// <summary>
        /// Gets information about the state of the device.
        /// </summary>
        DosDeviceInfo DeviceInfo { get; }
    }
}
