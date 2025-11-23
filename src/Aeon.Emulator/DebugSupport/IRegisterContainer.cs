namespace Aeon.Emulator.DebugSupport;

/// <summary>
/// Describes an object which contains x86 registers.
/// </summary>
public interface IRegisterContainer
{
    /// <summary>
    /// Gets the value of the EAX register.
    /// </summary>
    uint EAX { get; }
    /// <summary>
    /// Gets the value of the EBX register.
    /// </summary>
    uint EBX { get; }
    /// <summary>
    /// Gets the value of the ECX register.
    /// </summary>
    uint ECX { get; }
    /// <summary>
    /// Gets the value of the EDX register.
    /// </summary>
    uint EDX { get; }
    /// <summary>
    /// Gets the value of the ESI register.
    /// </summary>
    uint ESI { get; }
    /// <summary>
    /// Gets the value of the EDI register.
    /// </summary>
    uint EDI { get; }
    /// <summary>
    /// Gets the value of the EBP register.
    /// </summary>
    uint EBP { get; }
    /// <summary>
    /// Gets the value of the ESP register.
    /// </summary>
    uint ESP { get; }
    /// <summary>
    /// Gets the value of the DS register.
    /// </summary>
    ushort DS { get; }
    /// <summary>
    /// Gets the value of the ES register.
    /// </summary>
    ushort ES { get; }
    /// <summary>
    /// Gets the value of the FS register.
    /// </summary>
    ushort FS { get; }
    /// <summary>
    /// Gets the value of the GS register.
    /// </summary>
    ushort GS { get; }
    /// <summary>
    /// Gets the value of the SS register.
    /// </summary>
    ushort SS { get; }
    /// <summary>
    /// Gets the value of the flags register.
    /// </summary>
    EFlags Flags { get; }
    /// <summary>
    /// Gets the value of the CR0 register.
    /// </summary>
    CR0 CR0 { get; }
}
