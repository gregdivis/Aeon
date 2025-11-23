namespace Aeon.Emulator.Memory;

public enum DescriptorType
{
    Segment,
    CallGate,
    TaskGate,
    InterruptGate,
    TrapGate,
    TaskSegmentSelector
}
