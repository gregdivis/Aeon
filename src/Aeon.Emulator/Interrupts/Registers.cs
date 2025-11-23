namespace Aeon.Emulator;

/// <summary>
/// Specifies a set of registers.
/// </summary>
[Flags]
public enum Registers
{
    /// <summary>
    /// No registers.
    /// </summary>
    None = 0,
    /// <summary>
    /// The AX register.
    /// </summary>
    AX = 1 << 0,
    /// <summary>
    /// The BX register.
    /// </summary>
    BX = 1 << 1,
    /// <summary>
    /// The CX register.
    /// </summary>
    CX = 1 << 2,
    /// <summary>
    /// The DX register.
    /// </summary>
    DX = 1 << 3,
    /// <summary>
    /// The BP register.
    /// </summary>
    BP = 1 << 4,
    /// <summary>
    /// The SI register.
    /// </summary>
    SI = 1 << 5,
    /// <summary>
    /// The DI register.
    /// </summary>
    DI = 1 << 6,
    /// <summary>
    /// The DS register.
    /// </summary>
    DS = 1 << 7,
    /// <summary>
    /// The ES register.
    /// </summary>
    ES = 1 << 8
}
