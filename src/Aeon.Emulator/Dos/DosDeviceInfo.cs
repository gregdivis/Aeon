using System;

namespace Aeon.Emulator.Dos
{
    /// <summary>
    /// Provides information about the state of a DOS device.
    /// </summary>
    [Flags]
    public enum DosDeviceInfo
    {
        /// <summary>
        /// The device has no special flags set.
        /// </summary>
        Default = 0,
        /// <summary>
        /// The device is a console input device.
        /// </summary>
        ConsoleInputDevice = (1 << 0),
        /// <summary>
        /// The device is a console output device.
        /// </summary>
        ConsoleOutputDevice = (1 << 1),
        /// <summary>
        /// The device is the null device.
        /// </summary>
        NullDevice = (1 << 2),
        /// <summary>
        /// The device is the clock.
        /// </summary>
        ClockDevice = (1 << 3),
        /// <summary>
        /// The device is special.
        /// </summary>
        SpecialDevice = (1 << 4),
        /// <summary>
        /// The device is in binary mode.
        /// </summary>
        BinaryMode = (1 << 5),
        /// <summary>
        /// The device is not current at the end of its stream.
        /// </summary>
        NotEndOfFile = (1 << 6)
    }
}
