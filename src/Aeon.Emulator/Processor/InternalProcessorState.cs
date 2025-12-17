namespace Aeon.Emulator;

internal sealed class InternalProcessorState(ref readonly GeneralPurposeRegisters gpr, ref readonly SegmentRegisterBlock<ushort> segmentRegisters, ref readonly SegmentRegisterBlock<uint> segmentBases, EFlags flags)
{
    public GeneralPurposeRegisters GeneralPurposeRegisters { get; } = gpr;
    public SegmentRegisterBlock<ushort> SegmentRegisters { get; } = segmentRegisters;
    public SegmentRegisterBlock<uint> SegmentBases { get; } = segmentBases;
    public EFlags Flags { get; } = flags;
}
