namespace Aeon.Emulator;

/// <summary>
/// Represents the allowable values of the CR0 register.
/// </summary>
[Flags]
public enum CR0
{
    /// <summary>
    /// No flags are set.
    /// </summary>
    None = 0,
    /// <summary>
    /// Protected mode is enabled.
    /// </summary>
    ProtectedModeEnable = (1 << 0),
    /// <summary>
    /// Controls behavior of WAIT and FWAIT instructions.
    /// </summary>
    MonitorCoprocessor = (1 << 1),
    /// <summary>
    /// Indicates if FPU emulation is present.
    /// </summary>
    FloatingPointEmulation = (1 << 2),
    /// <summary>
    /// Saves FPU context after a task switch.
    /// </summary>
    TaskSwitched = (1 << 3),
    /// <summary>
    /// Unknown.
    /// </summary>
    ExtensionType = (1 << 4),
    /// <summary>
    /// Enables floating point exceptions.
    /// </summary>
    NumericError = (1 << 5),
    /// <summary>
    /// Unknown.
    /// </summary>
    WriteProtect = (1 << 16),
    /// <summary>
    /// Alignment check is enabled.
    /// </summary>
    AlignmentMask = (1 << 18),
    /// <summary>
    /// Unknown.
    /// </summary>
    NotWriteThrough = (1 << 29),
    /// <summary>
    /// Unknown.
    /// </summary>
    CacheDisabled = (1 << 30),
    /// <summary>
    /// Paging is enabled.
    /// </summary>
    Paging = (1 << 31)
}
