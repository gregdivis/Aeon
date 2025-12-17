using System.Runtime.CompilerServices;
using Aeon.Emulator.DebugSupport;

namespace Aeon.Emulator;

/// <summary>
/// Contains the current state of the emulated x86 processor.
/// </summary>
public sealed class Processor : IRegisterContainer
{
    private GeneralPurposeRegisters gpr;
    private SegmentRegisterBlock<ushort> segmentRegisters;
    private SegmentRegisterBlock<uint> segmentBases;
    private DebugRegisterBlock debugRegisters;
    private PrefixOverrides overrides;
    private CachedInstruction instructionBuffer;

    internal Processor()
    {
    }

    #region General Purpose
    /// <summary>
    /// Gets or sets the value of the EAX register.
    /// </summary>
    public ref int EAX => ref this.gpr.EAX;
    /// <summary>
    /// Gets the value of the EAX register.
    /// </summary>
    uint IRegisterContainer.EAX => (uint)this.EAX;
    /// <summary>
    /// Gets or sets the value of the EBX register.
    /// </summary>
    public ref int EBX => ref this.gpr.EBX;
    /// <summary>
    /// Gets the value of the EAX register.
    /// </summary>
    uint IRegisterContainer.EBX => (uint)this.EBX;
    /// <summary>
    /// Gets or sets the value of the ECX register.
    /// </summary>
    public ref int ECX => ref this.gpr.ECX;
    /// <summary>
    /// Gets the value of the EAX register.
    /// </summary>
    uint IRegisterContainer.ECX => (uint)this.ECX;
    /// <summary>
    /// Gets or sets the value of the EDX register.
    /// </summary>
    public ref int EDX => ref this.gpr.EDX;
    /// <summary>
    /// Gets the value of the EAX register.
    /// </summary>
    uint IRegisterContainer.EDX => (uint)this.EDX;
    /// <summary>
    /// Gets or sets the value of the AX register.
    /// </summary>
    public ref short AX => ref this.gpr.AX;
    /// <summary>
    /// Gets or sets the value of the BX register.
    /// </summary>
    public ref short BX => ref this.gpr.BX;
    /// <summary>
    /// Gets or sets the value of the CX register.
    /// </summary>
    public ref short CX => ref this.gpr.CX;
    /// <summary>
    /// Gets or sets the value of the DX register.
    /// </summary>
    public ref short DX => ref this.gpr.DX;

    /// <summary>
    /// Gets or sets the value of the AL register.
    /// </summary>
    public ref byte AL => ref this.gpr.AL;
    /// <summary>
    /// Gets or sets the value of the AH register.
    /// </summary>
    public ref byte AH => ref this.gpr.AH;
    /// <summary>
    /// Gets or sets the value of the BL register.
    /// </summary>
    public ref byte BL => ref this.gpr.BL;
    /// <summary>
    /// Gets or sets the value of the BH register.
    /// </summary>
    public ref byte BH => ref this.gpr.BH;
    /// <summary>
    /// Gets or sets the value of the CL register.
    /// </summary>
    public ref byte CL => ref this.gpr.CL;
    /// <summary>
    /// Gets or sets the value of the CH register.
    /// </summary>
    public ref byte CH => ref this.gpr.CH;
    /// <summary>
    /// Gets or sets the value of the DL register.
    /// </summary>
    public ref byte DL => ref this.gpr.DL;
    /// <summary>
    /// Gets or sets the value of the DH register.
    /// </summary>
    public ref byte DH => ref this.gpr.DH;
    #endregion

    #region Pointers
    /// <summary>
    /// Gets or sets the value of the EBP register.
    /// </summary>
    public ref uint EBP => ref this.gpr.EBP;
    uint IRegisterContainer.EBP => this.EBP;
    /// <summary>
    /// Gets or sets the value of the ESI register.
    /// </summary>
    public ref uint ESI => ref this.gpr.ESI;
    uint IRegisterContainer.ESI => this.ESI;
    /// <summary>
    /// Gets or sets the value of the EDI register.
    /// </summary>
    public ref uint EDI => ref this.gpr.EDI;
    uint IRegisterContainer.EDI => this.EDI;
    /// <summary>
    /// Gets or sets the value of the EIP register.
    /// </summary>
    public ref uint EIP => ref this.gpr.EIP;
    /// <summary>
    /// Gets or sets the value of the ESP register.
    /// </summary>
    public ref uint ESP => ref this.gpr.ESP;
    uint IRegisterContainer.ESP => this.ESP;

    /// <summary>
    /// Gets or sets the value of the BP register.
    /// </summary>
    public ref ushort BP => ref this.gpr.BP;
    /// <summary>
    /// Gets or sets the value of the SI register.
    /// </summary>
    public ref ushort SI => ref this.gpr.SI;
    /// <summary>
    /// Gets or sets the value of the DI register.
    /// </summary>
    public ref ushort DI => ref this.gpr.DI;
    /// <summary>
    /// Gets or sets the value of the IP register.
    /// </summary>
    public ref ushort IP => ref this.gpr.IP;
    /// <summary>
    /// Gets or sets the value of the SP register.
    /// </summary>
    public ref ushort SP => ref this.gpr.SP;
    #endregion

    #region Segment Registers
    /// <summary>
    /// Gets or sets the value of the ES register.
    /// </summary>
    public ushort ES => this.segmentRegisters.ES;
    /// <summary>
    /// Gets or sets the value of the CS register.
    /// </summary>
    public ushort CS => this.segmentRegisters.CS;
    /// <summary>
    /// Gets or sets the value of the SS register.
    /// </summary>
    public ushort SS => this.segmentRegisters.SS;
    /// <summary>
    /// Gets or sets the value of the DS register.
    /// </summary>
    public ushort DS => this.segmentRegisters.DS;
    /// <summary>
    /// Gets or sets the value of the FS register.
    /// </summary>
    public ushort FS => this.segmentRegisters.FS;
    /// <summary>
    /// Gets or sets the value of the GS register.
    /// </summary>
    public ushort GS => this.segmentRegisters.GS;

    /// <summary>
    /// Gets the current base address associated with the ES register.
    /// </summary>
    public uint ESBase => this.segmentBases.ES;
    /// <summary>
    /// Gets the current base address associated with the CS register.
    /// </summary>
    public uint CSBase => this.segmentBases.CS;
    /// <summary>
    /// Gets the current base address associated with the SS register.
    /// </summary>
    public uint SSBase => this.segmentBases.SS;
    /// <summary>
    /// Gets the current base address associated with the DS register.
    /// </summary>
    public uint DSBase => this.segmentBases.DS;
    /// <summary>
    /// Gets the current base address associated with the FS register.
    /// </summary>
    public uint FSBase => this.segmentBases.FS;
    /// <summary>
    /// Gets the current base address associated with the GS register.
    /// </summary>
    public uint GSBase => this.segmentBases.GS;
    #endregion

    public readonly FlagState Flags = new();
    /// <summary>
    /// Gets the value of the EFLAGS register.
    /// </summary>
    EFlags IRegisterContainer.Flags => this.Flags.Value;
    /// <summary>
    /// The current segment override prefix.
    /// </summary>
    public SegmentRegisterOverride SegmentOverride
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.overrides.Segment;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => this.overrides.Segment = value;
    }
    /// <summary>
    /// The current instruction repeat prefix.
    /// </summary>
    public RepeatPrefix RepeatPrefix
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.overrides.Repeat;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => this.overrides.Repeat = value;
    }
    /// <summary>
    /// The value of the CR0 register.
    /// </summary>
    public CR0 CR0;
    /// <summary>
    /// Gets the value of the CR0 register.
    /// </summary>
    CR0 IRegisterContainer.CR0 => this.CR0;
    /// <summary>
    /// The value of the CR2 register.
    /// </summary>
    public uint CR2;
    /// <summary>
    /// The value of the CR3 register.
    /// </summary>
    public uint CR3;
    /// <summary>
    /// Gets the width of the current operands in bits.
    /// </summary>
    public int OperandSize
    {
        get
        {
            uint bit = ((uint)this.GlobalSize ^ this.SizeOverride) & 1u;
            return bit == 0 ? 16 : 32;
        }
    }
    /// <summary>
    /// Gets the width of the current addressing mode in bits.
    /// </summary>
    public int AddressSize
    {
        get
        {
            uint bit = ((uint)this.GlobalSize ^ this.SizeOverride) & 2u;
            return bit == 0 ? 16 : 32;
        }
    }

    /// <summary>
    /// The floating-point unit.
    /// </summary>
    public readonly FPU FPU = new();

    #region Internal Properties
    /// <summary>
    /// Gets the current index to use for decoders and emulators.
    /// </summary>
    internal uint SizeModeIndex
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (uint)this.SizeOverride ^ this.GlobalSize;
    }

    /// <summary>
    /// Gets a value indicating whether an instruction prefix is in effect.
    /// </summary>
    internal bool InPrefix
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.PrefixCount != 0;
    }
    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal uint GetOverrideBase(SegmentIndex defaultSegment)
    {
        var segmentOverride = this.SegmentOverride;
        if (segmentOverride == SegmentRegisterOverride.None)
            return GetSegmentBasePointer((int)defaultSegment);
        else
            return GetSegmentBasePointer((int)segmentOverride - 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ref byte GetByteRegister(int rmCode) => ref Unsafe.AddByteOffset(ref this.AL, ((rmCode & 0b011) * 4) + (rmCode >>> 2));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ref T GetWordRegister<T>(int rmCode) where T : unmanaged => ref Unsafe.As<int, T>(ref Unsafe.Add(ref this.gpr.EAX, rmCode));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ref ushort GetSegmentRegisterPointer(int code) => ref Unsafe.Add(ref this.segmentRegisters.ES, code);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ref uint GetSegmentBasePointer(int code) => ref Unsafe.Add(ref this.segmentBases.ES, code);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ref uint GetDebugRegisterPointer(int code) => ref Unsafe.Add(ref this.debugRegisters.DR0, code);

    /// <summary>
    /// Clears prefix information after an instruction.
    /// </summary>
    /// <remarks>
    /// This method must be called explicitly by instructions with no operands.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void InstructionEpilog() => this.overrides = default;

    internal InternalProcessorState GetCurrentState() => new(in this.gpr, in this.segmentRegisters, in this.segmentBases, this.Flags.Value);
    internal void SetCurrentState(InternalProcessorState state)
    {
        this.gpr = state.GeneralPurposeRegisters;
        this.segmentRegisters = state.SegmentRegisters;
        this.segmentBases = state.SegmentBases;
        this.Flags.Value = state.Flags;
    }

    /// <summary>
    /// Contains operand size (bit 0) and address size (bit 1) overrides set
    /// by instruction prefixes.
    /// </summary>
    internal byte SizeOverride
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.overrides.Size;
    }
    /// <summary>
    /// Contains operand size (bit 0) and address size (bit 1) for the
    /// processor's default state.
    /// </summary>
    internal byte GlobalSize;
    /// <summary>
    /// The number of instruction prefixes currently in effect.
    /// </summary>
    internal uint PrefixCount
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.overrides.Count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void IncrementPrefixCount() => this.overrides.IncrementCount();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetSizeOverrideFlag(byte flag) => this.overrides.SetSizeFlag(flag);

    internal ref byte InstructionBuffer => ref this.instructionBuffer.a;

    /// <summary>
    /// 16-byte cache of the current instruction.
    /// </summary>
    internal ref CachedInstruction CachedInstruction => ref this.instructionBuffer;
    /// <summary>
    /// Pointer to the next byte in the cached instruction buffer.
    /// </summary>
    internal ref readonly byte CachedIP => ref Unsafe.AddByteOffset(ref Unsafe.As<CachedInstruction, byte>(ref this.instructionBuffer), this.EIP - this.StartEIP);
    /// <summary>
    /// Instruction pointer for the first byte of the current instruction.
    /// </summary>
    internal uint StartEIP;
    /// <summary>
    /// Specifies whether interrupts are disabled for the next instruction.
    /// </summary>
    internal bool TemporaryInterruptMask
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.overrides.InterruptMask;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => this.overrides.InterruptMask = value;
    }

    private struct PrefixOverrides
    {
        public SegmentRegisterOverride Segment;
        public byte Size;
        public byte Count;
        private byte repeatAndInterruptMask;

        public RepeatPrefix Repeat
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => (RepeatPrefix)(this.repeatAndInterruptMask & 0b11);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => this.repeatAndInterruptMask = (byte)((this.repeatAndInterruptMask & 0b100u) | (uint)value);
        }
        public bool InterruptMask
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => (this.repeatAndInterruptMask & 0b100) == 0b100;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (value)
                    this.repeatAndInterruptMask |= 0b100;
                else
                    this.repeatAndInterruptMask &= 0b011;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementCount() => this.Count++;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSizeFlag(byte flag) => this.Size |= flag;
    }
}
