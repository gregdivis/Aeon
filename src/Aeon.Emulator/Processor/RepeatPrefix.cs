namespace Aeon.Emulator
{
    /// <summary>
    /// Represents the possible repeat prefix modes.
    /// </summary>
    public enum RepeatPrefix : byte
    {
        /// <summary>
        /// No repeat prefix is in effect.
        /// </summary>
        None,
        /// <summary>
        /// The repeat while not equal prefix is in effect.
        /// </summary>
        Repne,
        /// <summary>
        /// The repeat while equal prefix is in effect.
        /// </summary>
        Repe
    }
}
