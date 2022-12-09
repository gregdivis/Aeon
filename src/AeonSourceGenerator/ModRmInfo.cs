namespace Aeon.SourceGenerator
{
    /// <summary>
    /// Describes the ModR/M byte of an instruction.
    /// </summary>
    public enum ModRmInfo : byte
    {
        /// <summary>
        /// The ModR/M byte is not present.
        /// </summary>
        None,
        /// <summary>
        /// Only the R/M field in the ModR/M byte is used.
        /// </summary>
        OnlyRm,
        /// <summary>
        /// The entire ModR/M byte is used.
        /// </summary>
        All,
        /// <summary>
        /// A register code is added to the opcode.
        /// </summary>
        RegisterPlus
    }
}
