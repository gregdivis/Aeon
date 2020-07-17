namespace AeonSourceGenerator
{
    /// <summary>
    /// Describes the type of an instruction operand.
    /// </summary>
    public enum OperandType : byte
    {
        /// <summary>
        /// The operand is not present.
        /// </summary>
        None,
        /// <summary>
        /// The operand is a byte register.
        /// </summary>
        RegisterByte,
        /// <summary>
        /// The operand is an immediate byte.
        /// </summary>
        ImmediateByte,
        /// <summary>
        /// The operand is an immediate byte that should be sign-extended to a word when it is read.
        /// </summary>
        ImmediateByteExtend,
        /// <summary>
        /// The operand is a word register.
        /// </summary>
        RegisterWord,
        /// <summary>
        /// The operand is an immediate word.
        /// </summary>
        ImmediateWord,
        /// <summary>
        /// The operand is either a byte register or a byte in memory.
        /// </summary>
        RegisterOrMemoryByte,
        /// <summary>
        /// The operand is either a word register or a word in memory.
        /// </summary>
        RegisterOrMemoryWord,
        /// <summary>
        /// The operand is a byte offset in memory.
        /// </summary>
        MemoryOffsetByte,
        /// <summary>
        /// The operand is a word offset in memory.
        /// </summary>
        MemoryOffsetWord,
        /// <summary>
        /// The operand is a segment register.
        /// </summary>
        SegmentRegister,
        /// <summary>
        /// The operand is the AL register.
        /// </summary>
        RegisterAL,
        /// <summary>
        /// The operand is the AH register.
        /// </summary>
        RegisterAH,
        /// <summary>
        /// The operand is the AX register.
        /// </summary>
        RegisterAX,
        /// <summary>
        /// The operand is the BL register.
        /// </summary>
        RegisterBL,
        /// <summary>
        /// The operand is the BH register.
        /// </summary>
        RegisterBH,
        /// <summary>
        /// The operand is the BX register.
        /// </summary>
        RegisterBX,
        /// <summary>
        /// The operand is the CL register.
        /// </summary>
        RegisterCL,
        /// <summary>
        /// The operand is the CH register.
        /// </summary>
        RegisterCH,
        /// <summary>
        /// The operand is the CX register.
        /// </summary>
        RegisterCX,
        /// <summary>
        /// The operand is the DL register.
        /// </summary>
        RegisterDL,
        /// <summary>
        /// The operand is the DH register.
        /// </summary>
        RegisterDH,
        /// <summary>
        /// The operand is the DX register.
        /// </summary>
        RegisterDX,
        /// <summary>
        /// The operand is the SP register.
        /// </summary>
        RegisterSP,
        /// <summary>
        /// The operand is the BP register.
        /// </summary>
        RegisterBP,
        /// <summary>
        /// The operand is the SI register.
        /// </summary>
        RegisterSI,
        /// <summary>
        /// The operand is the DI register.
        /// </summary>
        RegisterDI,
        /// <summary>
        /// The operand is the CS register.
        /// </summary>
        RegisterCS,
        /// <summary>
        /// The operand is the SS register.
        /// </summary>
        RegisterSS,
        /// <summary>
        /// The operand is the DS register.
        /// </summary>
        RegisterDS,
        /// <summary>
        /// The operand is the ES register.
        /// </summary>
        RegisterES,
        /// <summary>
        /// The operand is the FS register.
        /// </summary>
        RegisterFS,
        /// <summary>
        /// The operand is the GS register.
        /// </summary>
        RegisterGS,
        /// <summary>
        /// The operand is an immediate byte which should be added to CS+IP.
        /// </summary>
        ImmediateRelativeByte,
        /// <summary>
        /// The operand is an immediate word which should be added to CS+IP.
        /// </summary>
        ImmediateRelativeWord,
        /// <summary>
        /// The operand is a word register/memory address of a near pointer (CS+ptr).
        /// </summary>
        RegisterOrMemoryWordNearPointer,
        /// <summary>
        /// The operand is an immediate dword far pointer.
        /// </summary>
        ImmediateFarPointer,
        /// <summary>
        /// The operand is a register/memory address of a dword far pointer.
        /// </summary>
        IndirectFarPointer,
        /// <summary>
        /// The operand is a memory word/byte and should contain the effective address.
        /// </summary>
        EffectiveAddress,
        /// <summary>
        /// The operand is a memory word/byte and should contain the effective address.
        /// </summary>
        FullLinearAddress,
        /// <summary>
        /// The operand is an immediate 16-bit integer.
        /// </summary>
        ImmediateInt16,
        /// <summary>
        /// The operand is an immediate 32-bit integer.
        /// </summary>
        ImmediateInt32,
        /// <summary>
        /// The operand is an immediate 64-bit integer.
        /// </summary>
        ImmediateInt64,
        /// <summary>
        /// The operand is the address of a 16-bit integer.
        /// </summary>
        MemoryInt16,
        /// <summary>
        /// The operand is the address of a 32-bit integer.
        /// </summary>
        MemoryInt32,
        /// <summary>
        /// The operand is the address of a 64-bit integer.
        /// </summary>
        MemoryInt64,
        /// <summary>
        /// The operand is either a 16-bit register or a 16-bit value in memory.
        /// </summary>
        RegisterOrMemory16,
        /// <summary>
        /// The operand is either a 32-bit register or a 32-bit value in memory.
        /// </summary>
        RegisterOrMemory32,
        /// <summary>
        /// The operand is the address of a 32-bit floating-point value.
        /// </summary>
        MemoryFloat32,
        /// <summary>
        /// The operand is the address of a 64-bit floating-point value.
        /// </summary>
        MemoryFloat64,
        /// <summary>
        /// The operand is the address of an 80-bit floating-point value.
        /// </summary>
        MemoryFloat80,
        /// <summary>
        /// The operand is an FPU ST register.
        /// </summary>
        RegisterST,
        /// <summary>
        /// The operand is the ST0 register.
        /// </summary>
        RegisterST0,
        /// <summary>
        /// The operand is the ST1 register.
        /// </summary>
        RegisterST1,
        /// <summary>
        /// The operand is the ST2 register.
        /// </summary>
        RegisterST2,
        /// <summary>
        /// The operand is the ST3 register.
        /// </summary>
        RegisterST3,
        /// <summary>
        /// The operand is the ST4 register.
        /// </summary>
        RegisterST4,
        /// <summary>
        /// The operand is the ST5 register.
        /// </summary>
        RegisterST5,
        /// <summary>
        /// The operand is the ST6 register.
        /// </summary>
        RegisterST6,
        /// <summary>
        /// The operand is the ST7 register.
        /// </summary>
        RegisterST7,
        /// <summary>
        /// The operand is a debug register.
        /// </summary>
        DebugRegister
    }
}
