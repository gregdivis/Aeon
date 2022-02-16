namespace Aeon.Emulator.Launcher.Debugger
{
    /// <summary>
    /// Specifies the type of a target address.
    /// </summary>
    public enum TargetAddressType
    {
        /// <summary>
        /// The address refers to a code segment.
        /// </summary>
        Code,
        /// <summary>
        /// The address refers to a data segment.
        /// </summary>
        Data
    }
}
