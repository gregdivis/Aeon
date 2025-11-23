namespace Aeon.Emulator.Decoding;

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
    [OperandString("rb")]
    RegisterByte,
    /// <summary>
    /// The operand is an immediate byte.
    /// </summary>
    [OperandString("ib")]
    ImmediateByte,
    /// <summary>
    /// The operand is an immediate byte that should be sign-extended to a word when it is read.
    /// </summary>
    [OperandString("ibx")]
    ImmediateByteExtend,
    /// <summary>
    /// The operand is a word register.
    /// </summary>
    [OperandString("rw")]
    RegisterWord,
    /// <summary>
    /// The operand is an immediate word.
    /// </summary>
    [OperandString("iw")]
    ImmediateWord,
    /// <summary>
    /// The operand is either a byte register or a byte in memory.
    /// </summary>
    [OperandString("rmb")]
    RegisterOrMemoryByte,
    /// <summary>
    /// The operand is either a word register or a word in memory.
    /// </summary>
    [OperandString("rmw")]
    RegisterOrMemoryWord,
    /// <summary>
    /// The operand is a byte offset in memory.
    /// </summary>
    [OperandString("moffsb")]
    MemoryOffsetByte,
    /// <summary>
    /// The operand is a word offset in memory.
    /// </summary>
    [OperandString("moffsw")]
    MemoryOffsetWord,
    /// <summary>
    /// The operand is a segment register.
    /// </summary>
    [OperandString("sreg")]
    SegmentRegister,
    /// <summary>
    /// The operand is the AL register.
    /// </summary>
    [OperandString("al")]
    RegisterAL,
    /// <summary>
    /// The operand is the AH register.
    /// </summary>
    [OperandString("ah")]
    RegisterAH,
    /// <summary>
    /// The operand is the AX register.
    /// </summary>
    [OperandString("ax")]
    RegisterAX,
    /// <summary>
    /// The operand is the BL register.
    /// </summary>
    [OperandString("bl")]
    RegisterBL,
    /// <summary>
    /// The operand is the BH register.
    /// </summary>
    [OperandString("bh")]
    RegisterBH,
    /// <summary>
    /// The operand is the BX register.
    /// </summary>
    [OperandString("bx")]
    RegisterBX,
    /// <summary>
    /// The operand is the CL register.
    /// </summary>
    [OperandString("cl")]
    RegisterCL,
    /// <summary>
    /// The operand is the CH register.
    /// </summary>
    [OperandString("ch")]
    RegisterCH,
    /// <summary>
    /// The operand is the CX register.
    /// </summary>
    [OperandString("cx")]
    RegisterCX,
    /// <summary>
    /// The operand is the DL register.
    /// </summary>
    [OperandString("dl")]
    RegisterDL,
    /// <summary>
    /// The operand is the DH register.
    /// </summary>
    [OperandString("dh")]
    RegisterDH,
    /// <summary>
    /// The operand is the DX register.
    /// </summary>
    [OperandString("dx")]
    RegisterDX,
    /// <summary>
    /// The operand is the SP register.
    /// </summary>
    [OperandString("sp")]
    RegisterSP,
    /// <summary>
    /// The operand is the BP register.
    /// </summary>
    [OperandString("bp")]
    RegisterBP,
    /// <summary>
    /// The operand is the SI register.
    /// </summary>
    [OperandString("si")]
    RegisterSI,
    /// <summary>
    /// The operand is the DI register.
    /// </summary>
    [OperandString("di")]
    RegisterDI,
    /// <summary>
    /// The operand is the CS register.
    /// </summary>
    [OperandString("cs")]
    RegisterCS,
    /// <summary>
    /// The operand is the SS register.
    /// </summary>
    [OperandString("ss")]
    RegisterSS,
    /// <summary>
    /// The operand is the DS register.
    /// </summary>
    [OperandString("ds")]
    RegisterDS,
    /// <summary>
    /// The operand is the ES register.
    /// </summary>
    [OperandString("es")]
    RegisterES,
    /// <summary>
    /// The operand is the FS register.
    /// </summary>
    [OperandString("fs")]
    RegisterFS,
    /// <summary>
    /// The operand is the GS register.
    /// </summary>
    [OperandString("gs")]
    RegisterGS,
    /// <summary>
    /// The operand is an immediate byte which should be added to CS+IP.
    /// </summary>
    [OperandString("irelb")]
    ImmediateRelativeByte,
    /// <summary>
    /// The operand is an immediate word which should be added to CS+IP.
    /// </summary>
    [OperandString("irelw")]
    ImmediateRelativeWord,
    /// <summary>
    /// The operand is a word register/memory address of a near pointer (CS+ptr).
    /// </summary>
    [OperandString("jmprmw")]
    RegisterOrMemoryWordNearPointer,
    /// <summary>
    /// The operand is an immediate dword far pointer.
    /// </summary>
    [OperandString("iptr")]
    ImmediateFarPointer,
    /// <summary>
    /// The operand is a register/memory address of a dword far pointer.
    /// </summary>
    [OperandString("mptr")]
    IndirectFarPointer,
    /// <summary>
    /// The operand is a memory word/byte and should contain the effective address.
    /// </summary>
    [OperandString("addr:rmw")]
    EffectiveAddress,
    /// <summary>
    /// The operand is a memory word/byte and should contain the effective address.
    /// </summary>
    [OperandString("fulladdr:rmw")]
    FullLinearAddress,
    /// <summary>
    /// The operand is an immediate 16-bit integer.
    /// </summary>
    [OperandString("i16")]
    ImmediateInt16,
    /// <summary>
    /// The operand is an immediate 32-bit integer.
    /// </summary>
    [OperandString("i32")]
    ImmediateInt32,
    /// <summary>
    /// The operand is an immediate 64-bit integer.
    /// </summary>
    [OperandString("i64")]
    ImmediateInt64,
    /// <summary>
    /// The operand is the address of a 16-bit integer.
    /// </summary>
    [OperandString("m16")]
    MemoryInt16,
    /// <summary>
    /// The operand is the address of a 32-bit integer.
    /// </summary>
    [OperandString("m32")]
    MemoryInt32,
    /// <summary>
    /// The operand is the address of a 64-bit integer.
    /// </summary>
    [OperandString("m64")]
    MemoryInt64,
    /// <summary>
    /// The operand is either a 16-bit register or a 16-bit value in memory.
    /// </summary>
    [OperandString("rm16")]
    RegisterOrMemory16,
    /// <summary>
    /// The operand is either a 32-bit register or a 32-bit value in memory.
    /// </summary>
    [OperandString("rm32")]
    RegisterOrMemory32,
    /// <summary>
    /// The operand is the address of a 32-bit floating-point value.
    /// </summary>
    [OperandString("mf32")]
    MemoryFloat32,
    /// <summary>
    /// The operand is the address of a 64-bit floating-point value.
    /// </summary>
    [OperandString("mf64")]
    MemoryFloat64,
    /// <summary>
    /// The operand is the address of an 80-bit floating-point value.
    /// </summary>
    [OperandString("mf80")]
    MemoryFloat80,
    /// <summary>
    /// The operand is an FPU ST register.
    /// </summary>
    [OperandString("st")]
    RegisterST,
    /// <summary>
    /// The operand is the ST0 register.
    /// </summary>
    [OperandString("st0")]
    RegisterST0,
    /// <summary>
    /// The operand is the ST1 register.
    /// </summary>
    [OperandString("st1")]
    RegisterST1,
    /// <summary>
    /// The operand is the ST2 register.
    /// </summary>
    [OperandString("st2")]
    RegisterST2,
    /// <summary>
    /// The operand is the ST3 register.
    /// </summary>
    [OperandString("st3")]
    RegisterST3,
    /// <summary>
    /// The operand is the ST4 register.
    /// </summary>
    [OperandString("st4")]
    RegisterST4,
    /// <summary>
    /// The operand is the ST5 register.
    /// </summary>
    [OperandString("st5")]
    RegisterST5,
    /// <summary>
    /// The operand is the ST6 register.
    /// </summary>
    [OperandString("st6")]
    RegisterST6,
    /// <summary>
    /// The operand is the ST7 register.
    /// </summary>
    [OperandString("st7")]
    RegisterST7,
    /// <summary>
    /// The operand is a debug register.
    /// </summary>
    [OperandString("dr")]
    DebugRegister
}
