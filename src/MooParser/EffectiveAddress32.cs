namespace MooParser;

public sealed record EffectiveAddress32(SegmentRegister Register, ushort SegmentSelector, uint SegmentBaseAddress, uint SegmentLimit, uint Offset, uint LinearAddress, uint PhysicalAddress);
