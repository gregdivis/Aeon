namespace Aeon.Emulator.Instructions;

/// <summary>
/// Marks an alternative method to use for emulating an opcode when
/// the operand size or address size is not 16 bits.
/// </summary>
/// <remarks>
/// Initializes a new instance of the AlternateAttribute class.
/// </remarks>
/// <param name="methodName">Name of the default method for this opcode.</param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal sealed class AlternateAttribute(string methodName) : Attribute
{
    /// <summary>
    /// Gets the name of the default method for this opcode.
    /// </summary>
    public string MethodName { get; } = methodName;
    /// <summary>
    /// Gets or sets the operand sizes that the method accepts.
    /// </summary>
    public int OperandSize { get; set; } = 32;
    /// <summary>
    /// Gets or sets the address sizes that the method accepts.
    /// </summary>
    public int AddressSize { get; set; } = 16;
}
