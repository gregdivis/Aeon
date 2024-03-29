﻿#nullable enable


namespace Aeon.Emulator.Sound
{
    /// <summary>
    /// Specifies the current state of the General MIDI device.
    /// </summary>
    public enum GeneralMidiState
    {
        /// <summary>
        /// The device is in normal mode.
        /// </summary>
        NormalMode,
        /// <summary>
        /// The device is in UART mode.
        /// </summary>
        UartMode
    }
}
