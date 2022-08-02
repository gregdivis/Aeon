namespace Aeon.Emulator
{
    /// <summary>
    /// Describes the current state of the emulated system.
    /// </summary>
    public enum EmulatorState
    {
        /// <summary>
        /// The emulator is initialized but no program has been loaded yet.
        /// </summary>
        NoProgram,
        /// <summary>
        /// The emulator is ready to run.
        /// </summary>
        Ready,
        /// <summary>
        /// The emulator is running.
        /// </summary>
        Running,
        /// <summary>
        /// The emulator is paused.
        /// </summary>
        Paused,
        /// <summary>
        /// The emulator has reached the end of the currently loaded program.
        /// </summary>
        ProgramExited,
        /// <summary>
        /// The emulator has been halted and cannot be resumed.
        /// </summary>
        Halted
    }
}
