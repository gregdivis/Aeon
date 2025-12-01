namespace Aeon.Emulator.Instructions;

/// <summary>
/// Describes the machine code format expected for an instruction.
/// </summary>
/// <remarks>
/// Initializes a new instance of the OpcodeAttribute class.
/// </remarks>
/// <param name="opcodeFormat">Machine code format of the opcode and its operands.</param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal sealed class OpcodeAttribute(string opcodeFormat) : Attribute
{
    /// <summary>
    /// Gets the machine code format of the opcode and its operands.
    /// </summary>
    public string OpcodeFormat { get; } = opcodeFormat;
    /// <summary>
    /// Gets or sets the name of the opcode.
    /// </summary>
    public string? Name { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the opcode is a prefix.
    /// </summary>
    public bool IsPrefix { get; set; }
    /// <summary>
    /// Gets or sets the operand sizes that the method accepts.
    /// </summary>
    public int OperandSize { get; set; } = 16;
    /// <summary>
    /// Gets or sets the address sizes that the method accepts.
    /// </summary>
    public int AddressSize { get; set; } = 16;
}
