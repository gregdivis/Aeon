namespace Aeon.Emulator.Launcher
{
    /// <summary>
    /// Specifies the type of mouse input provided to the emulator.
    /// </summary>
    public enum MouseInputMode
    {
        /// <summary>
        /// Mouse movement is captured and reported to the emulator in relative units.
        /// </summary>
        Relative,
        /// <summary>
        /// Mouse movement is not captured and is reported to the emulator at absolute coordinates.
        /// </summary>
        Absolute
    }
}
