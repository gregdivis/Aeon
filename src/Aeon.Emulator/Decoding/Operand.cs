using Aeon.Emulator.DebugSupport;

namespace Aeon.Emulator;

/// <summary>
/// Operand information decoded from an instruction.
/// </summary>
public struct CodeOperand
{
    private CodeOperandType type;
    private CodeRegister register;
    private CodeMemoryBase memoryBase;
    private CodeOperandSize operandSize;
    private byte sibScale;
    private CodeSibRegister sibIndex;
    private CodeSibRegister sibBase;
    private uint value;
    private ushort farSegment;

    /// <summary>
    /// Initializes a new <see cref="CodeOperand"/> struct.
    /// </summary>
    /// <param name="type">Type of the operand.</param>
    /// <param name="value">Value of the operand.</param>
    public CodeOperand(CodeOperandType type, uint value, CodeOperandSize size)
    {
        this.type = type;
        this.value = value;
        this.operandSize = size;
        this.memoryBase = 0;
        this.register = 0;
        this.sibScale = 0;
        this.sibIndex = 0;
        this.sibBase = 0;
        this.farSegment = 0;
    }
    /// <summary>
    /// Initializes a new <see cref="CodeOperand"/> struct.
    /// </summary>
    /// <param name="effectiveAddress">Addressing mode for the operand.</param>
    /// <param name="value">Displacement value for the operand.</param>
    public CodeOperand(CodeMemoryBase effectiveAddress, uint value, CodeOperandSize size)
    {
        this.type = CodeOperandType.MemoryAddress;
        this.value = value;
        this.memoryBase = effectiveAddress;
        this.operandSize = size;
        this.register = 0;
        this.sibScale = 0;
        this.sibIndex = 0;
        this.sibBase = 0;
        this.farSegment = 0;
    }
    /// <summary>
    /// Initializes a new <see cref="CodeOperand"/> struct.
    /// </summary>
    /// <param name="value">Value of the operand as a register.</param>
    public CodeOperand(CodeRegister value)
    {
        this.type = CodeOperandType.Register;
        this.register = value;

        if (value == CodeRegister.AL || value == CodeRegister.AH || value == CodeRegister.BL || value == CodeRegister.BH || value == CodeRegister.CL || value == CodeRegister.CH || value == CodeRegister.DL || value == CodeRegister.DH)
            this.operandSize = CodeOperandSize.Byte;
        else if (value == CodeRegister.EAX || value == CodeRegister.EBX || value == CodeRegister.ECX || value == CodeRegister.EDX || value == CodeRegister.EBP || value == CodeRegister.ESI || value == CodeRegister.EDI || value == CodeRegister.ESP)
            this.operandSize = CodeOperandSize.DoubleWord;
        else
            this.operandSize = CodeOperandSize.Word;

        this.memoryBase = 0;
        this.value = 0;
        this.sibScale = 0;
        this.sibIndex = 0;
        this.sibBase = 0;
        this.farSegment = 0;
    }

    public static bool operator ==(CodeOperand opA, CodeOperand opB) => opA.Type == opB.Type && opA.ImmediateValue == opB.ImmediateValue && opA.RegisterValue == opB.RegisterValue;
    public static bool operator !=(CodeOperand opA, CodeOperand opB) => opA.Type != opB.Type || opA.ImmediateValue != opB.ImmediateValue || opA.RegisterValue != opB.RegisterValue;
    public static implicit operator CodeOperand(CodeRegister register) => new(register);

    /// <summary>
    /// Gets or sets the type of the operand.
    /// </summary>
    public CodeOperandType Type
    {
        readonly get => type;
        set => type = value;
    }
    /// <summary>
    /// Gets or sets the immediate/displacement value in the operand.
    /// </summary>
    public uint ImmediateValue
    {
        readonly get => value;
        set => this.value = value;
    }
    /// <summary>
    /// Gets or sets the register in the operand.
    /// </summary>
    public CodeRegister RegisterValue
    {
        readonly get => register;
        set => register = value;
    }
    /// <summary>
    /// Gets or sets the mode used to calculate the effective address.
    /// </summary>
    public CodeMemoryBase EffectiveAddress
    {
        readonly get => memoryBase;
        set => memoryBase = value;
    }
    /// <summary>
    /// Gets or sets the size of the operand's value.
    /// </summary>
    public CodeOperandSize OperandSize
    {
        readonly get => operandSize;
        set => operandSize = value;
    }
    /// <summary>
    /// Gets or sets the scale factor of the SIB byte.
    /// </summary>
    public byte Scale
    {
        readonly get => sibScale;
        set => sibScale = value;
    }
    /// <summary>
    /// Gets or sets the index register of the SIB byte.
    /// </summary>
    public CodeSibRegister Index
    {
        readonly get => sibIndex;
        set => sibIndex = value;
    }
    /// <summary>
    /// Gets or sets the base register of the SIB byte.
    /// </summary>
    public CodeSibRegister Base
    {
        readonly get => sibBase;
        set => sibBase = value;
    }
    /// <summary>
    /// Gets or sets the segment of a far address.
    /// </summary>
    public ushort FarSegment
    {
        readonly get => this.farSegment;
        set => this.farSegment = value;
    }

    /// <summary>
    /// Returns a <see cref="CodeOperand"/> containing a far pointer address.
    /// </summary>
    /// <param name="segment">The pointer segment.</param>
    /// <param name="offset">The pointer offset.</param>
    /// <returns><see cref="CodeOperand"/> containing the far pointer address.</returns>
    public static CodeOperand FarPointer(ushort segment, uint offset) => new() { type = CodeOperandType.FarMemoryAddress, value = offset, farSegment = segment };

    /// <summary>
    /// Gets a string representation of the operand.
    /// </summary>
    /// <returns>String representation of the operand.</returns>
    public override readonly string ToString() => this.ToString(0, PrefixState.None);
    public readonly string ToString(int ip, PrefixState prefixes)
    {
        switch (type)
        {
            case CodeOperandType.Register:
                return RegisterFormatter.Format(this.RegisterValue);

            case CodeOperandType.Immediate:
                return this.ImmediateValue.ToString("X");

            case CodeOperandType.MemoryAddress:
            case CodeOperandType.EffectiveAddress:
            case CodeOperandType.AbsoluteJumpAddress:
                return AddressFormatter.Format(this, prefixes);

            case CodeOperandType.IndirectFarMemoryAddress:
                return $"[{(ushort)(this.ImmediateValue & 0xFFFF):X4}]";

            case CodeOperandType.FarMemoryAddress:
                if ((prefixes & PrefixState.AddressSize) == 0)
                    return $"{this.FarSegment:X4}:{this.ImmediateValue:X4}";
                else
                    return $"{this.FarSegment:X4}:{this.ImmediateValue:X8}";

            case CodeOperandType.RelativeJumpAddress:
                if ((prefixes & PrefixState.AddressSize) == 0)
                    return (ip + (int)this.ImmediateValue).ToString("X4");
                else
                    return (ip + (int)this.ImmediateValue).ToString("X8");

            default:
                return string.Empty;
        }
    }
    public override readonly bool Equals(object? obj) => obj is CodeOperand other && (this == other);
    public override readonly int GetHashCode() => HashCode.Combine(this.type, this.register, this.value);
}

/// <summary>
/// Contains the decoded operands of an instruction.
/// </summary>
public struct DecodedOperands
{
    private CodeOperand operand1;
    private CodeOperand operand2;
    private CodeOperand operand3;

    /// <summary>
    /// Initializes a new <see cref="DecodedOperands"/> struct.
    /// </summary>
    /// <param name="operand1">Decoded first operand.</param>
    public DecodedOperands(CodeOperand operand1)
    {
        this.operand1 = operand1;
        this.operand2 = default;
        this.operand3 = default;
    }
    /// <summary>
    /// Initializes a new <see cref="DecodedOperands"/> struct.
    /// </summary>
    /// <param name="operand1">Decoded first operand.</param>
    /// <param name="operand2">Decoded second operand.</param>
    public DecodedOperands(CodeOperand operand1, CodeOperand operand2)
    {
        this.operand1 = operand1;
        this.operand2 = operand2;
        this.operand3 = default;
    }
    /// <summary>
    /// Initializes a new <see cref="DecodedOperands"/> struct.
    /// </summary>
    /// <param name="operand1">Decoded first operand.</param>
    /// <param name="operand2">Decoded second operand.</param>
    /// <param name="operand3">Decoded third operand.</param>
    public DecodedOperands(CodeOperand operand1, CodeOperand operand2, CodeOperand operand3)
    {
        this.operand1 = operand1;
        this.operand2 = operand2;
        this.operand3 = operand3;
    }

    public static bool operator ==(DecodedOperands valueA, DecodedOperands valueB) => valueA.Operand1 == valueB.Operand1 && valueA.Operand2 == valueB.Operand2 && valueA.Operand3 == valueB.Operand3;
    public static bool operator !=(DecodedOperands valueA, DecodedOperands valueB) => valueA.Operand1 != valueB.Operand1 || valueA.Operand2 != valueB.Operand2 || valueA.Operand3 != valueB.Operand3;

    /// <summary>
    /// Gets or sets the first decoded operand.
    /// </summary>
    public CodeOperand Operand1
    {
        readonly get => operand1;
        set => operand1 = value;
    }
    /// <summary>
    /// Gets or sets the second decoded operand.
    /// </summary>
    public CodeOperand Operand2
    {
        readonly get => operand2;
        set => operand2 = value;
    }
    /// <summary>
    /// Gets or sets the third decoded operand.
    /// </summary>
    public CodeOperand Operand3
    {
        readonly get => operand3;
        set => operand3 = value;
    }

    internal void SetOperand(int index, CodeOperand value)
    {
        switch (index)
        {
            case 0:
                this.operand1 = value;
                break;

            case 1:
                this.operand2 = value;
                break;

            case 2:
                this.operand3 = value;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(index));
        }
    }

    /// <summary>
    /// Gets a string representation of the decoded operands.
    /// </summary>
    /// <returns>String representation of the decoded operands.</returns>
    public override readonly string ToString()
    {
        if (operand1.Type == CodeOperandType.None)
            return string.Empty;
        else if (operand2.Type == CodeOperandType.None)
            return operand1.ToString();
        else if (operand3.Type == CodeOperandType.None)
            return operand1.ToString() + ", " + operand2.ToString();
        else
            return operand1.ToString() + ", " + operand2.ToString() + ", " + operand3.ToString();
    }
    public readonly string ToString(int ip, PrefixState prefixes)
    {
        if (operand1.Type == CodeOperandType.None)
            return string.Empty;
        else if (operand2.Type == CodeOperandType.None)
            return operand1.ToString(ip, prefixes);
        else if (operand3.Type == CodeOperandType.None)
            return operand1.ToString(ip, prefixes) + ", " + operand2.ToString(ip, prefixes);
        else
            return operand1.ToString(ip, prefixes) + ", " + operand2.ToString(ip, prefixes) + ", " + operand3.ToString(ip, prefixes);
    }
    public override readonly bool Equals(object? obj) => obj is DecodedOperands other && this == other;
    public override readonly int GetHashCode() => HashCode.Combine(operand1, operand2, operand3);
}

/// <summary>
/// Specifies the type of an operand.
/// </summary>
public enum CodeOperandType : byte
{
    /// <summary>
    /// The operand is not specified.
    /// </summary>
    None,
    /// <summary>
    /// The operand specifies a register.
    /// </summary>
    Register,
    /// <summary>
    /// The operand is an immediate value.
    /// </summary>
    Immediate,
    /// <summary>
    /// The operand is a memory address.
    /// </summary>
    MemoryAddress,
    /// <summary>
    /// The operand is a 32/48-bit far pointer.
    /// </summary>
    FarMemoryAddress,
    /// <summary>
    /// The operand is a pointer to a 32/48-bit memory address.
    /// </summary>
    IndirectFarMemoryAddress,
    /// <summary>
    /// The operand is a relative jump target address.
    /// </summary>
    RelativeJumpAddress,
    /// <summary>
    /// The operand is an absolute jump target address (same segment).
    /// </summary>
    AbsoluteJumpAddress,
    /// <summary>
    /// The operand is the effective address of a pointer.
    /// </summary>
    EffectiveAddress,
    FullLinearAddress
}

/// <summary>
/// Specifies a register in an operand.
/// </summary>
public enum CodeRegister : byte
{
    /// <summary>
    /// The AL register.
    /// </summary>
    AL,
    /// <summary>
    /// The AH register.
    /// </summary>
    AH,
    /// <summary>
    /// The BL register.
    /// </summary>
    BL,
    /// <summary>
    /// The BL register.
    /// </summary>
    BH,
    /// <summary>
    /// The CL register.
    /// </summary>
    CL,
    /// <summary>
    /// The CH register.
    /// </summary>
    CH,
    /// <summary>
    /// The DL register.
    /// </summary>
    DL,
    /// <summary>
    /// The DH register.
    /// </summary>
    DH,
    /// <summary>
    /// The AX register.
    /// </summary>
    AX,
    /// <summary>
    /// The BX register.
    /// </summary>
    BX,
    /// <summary>
    /// The CX register.
    /// </summary>
    CX,
    /// <summary>
    /// The DX register.
    /// </summary>
    DX,
    /// <summary>
    /// The EAX register.
    /// </summary>
    EAX,
    /// <summary>
    /// The EBX register.
    /// </summary>
    EBX,
    /// <summary>
    /// The ECX register.
    /// </summary>
    ECX,
    /// <summary>
    /// The EDX register.
    /// </summary>
    EDX,
    /// <summary>
    /// The SI register.
    /// </summary>
    SI,
    /// <summary>
    /// The DI register.
    /// </summary>
    DI,
    /// <summary>
    /// The SP register.
    /// </summary>
    SP,
    /// <summary>
    /// The BP register.
    /// </summary>
    BP,
    /// <summary>
    /// The ESI register.
    /// </summary>
    ESI,
    /// <summary>
    /// The EDI register.
    /// </summary>
    EDI,
    /// <summary>
    /// The ESP register.
    /// </summary>
    ESP,
    /// <summary>
    /// The EBP register.
    /// </summary>
    EBP,
    /// <summary>
    /// The CS register.
    /// </summary>
    CS,
    /// <summary>
    /// The SS register.
    /// </summary>
    SS,
    /// <summary>
    /// The DS register.
    /// </summary>
    DS,
    /// <summary>
    /// The ES register.
    /// </summary>
    ES,
    /// <summary>
    /// The FS register.
    /// </summary>
    FS,
    /// <summary>
    /// The GS register.
    /// </summary>
    GS
}

/// <summary>
/// Specifies a register in the SIB byte.
/// </summary>
public enum CodeSibRegister : byte
{
    /// <summary>
    /// No register.
    /// </summary>
    None,
    /// <summary>
    /// The EAX register.
    /// </summary>
    EAX,
    /// <summary>
    /// The ECX register.
    /// </summary>
    ECX,
    /// <summary>
    /// The EDX register.
    /// </summary>
    EDX,
    /// <summary>
    /// The EBX register.
    /// </summary>
    EBX,
    /// <summary>
    /// The EBP register.
    /// </summary>
    EBP,
    /// <summary>
    /// The ESI register.
    /// </summary>
    ESI,
    /// <summary>
    /// The EDI register.
    /// </summary>
    EDI,
    /// <summary>
    /// The ESP register.
    /// </summary>
    ESP
}

/// <summary>
/// Specifies an addressing mode for calculating the effective address of the operand.
/// </summary>
public enum CodeMemoryBase : byte
{
    /// <summary>
    /// The effective address is specified by the immediate value.
    /// </summary>
    DisplacementOnly,
    /// <summary>
    /// The effective adddress is BX+SI+immediate.
    /// </summary>
    BX_plus_SI,
    /// <summary>
    /// The effective address is BX+DI+immediate.
    /// </summary>
    BX_plus_DI,
    /// <summary>
    /// The effective address is BP+SI+immediate.
    /// </summary>
    BP_plus_SI,
    /// <summary>
    /// The effective address is BP+DI+immediate.
    /// </summary>
    BP_plus_DI,
    /// <summary>
    /// The effective address is SI+immediate.
    /// </summary>
    SI,
    /// <summary>
    /// The effective address is DI+immediate.
    /// </summary>
    DI,
    /// <summary>
    /// The effective address is BX+immediate.
    /// </summary>
    BX,
    /// <summary>
    /// The effective address is BP+immediate.
    /// </summary>
    BP,
    /// <summary>
    /// The effective address is EAX+immediate.
    /// </summary>
    EAX,
    /// <summary>
    /// The effective address is ECX+immediate.
    /// </summary>
    ECX,
    /// <summary>
    /// The effective address is EDX+immediate.
    /// </summary>
    EDX,
    /// <summary>
    /// The effective address is EBX+immediate.
    /// </summary>
    EBX,
    /// <summary>
    /// The effective address is EBP+immediate.
    /// </summary>
    EBP,
    /// <summary>
    /// The effective address is ESI+immediate.
    /// </summary>
    ESI,
    /// <summary>
    /// The effective address is EDI+immediate.
    /// </summary>
    EDI,
    /// <summary>
    /// The effective address is described by the SIB byte.
    /// </summary>
    SIB
}

/// <summary>
/// Specifies the size of an operand's value.
/// </summary>
public enum CodeOperandSize : byte
{
    /// <summary>
    /// The operand size is 1 byte.
    /// </summary>
    Byte,
    /// <summary>
    /// The operand size is 2 bytes.
    /// </summary>
    Word,
    /// <summary>
    /// The operand size is 4 bytes.
    /// </summary>
    DoubleWord,
    /// <summary>
    /// The operand size is 8 bytes.
    /// </summary>
    QuadWord,
    /// <summary>
    /// The operand size is 4 bytes (floating point).
    /// </summary>
    Single,
    /// <summary>
    /// The operand size is 8 bytes (floating point).
    /// </summary>
    Double,
    /// <summary>
    /// The operand size is 10 bytes (floating point).
    /// </summary>
    LongDouble
}

/// <summary>
/// Specifies the direction of data flow with regard to the operand.
/// </summary>
public enum CodeOperandFlow : byte
{
    /// <summary>
    /// The operand is an input.
    /// </summary>
    In,
    /// <summary>
    /// The operand is an output.
    /// </summary>
    Out,
    /// <summary>
    /// The operand is an input and and output.
    /// </summary>
    InOut
}
