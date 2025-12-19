namespace Aeon.Emulator.Memory;

public enum DescriptorType
{
    Segment,
    CallGate,
    Ldt,
    TaskGate,
    InterruptGate,
    TrapGate,
    TaskSegmentSelector
}
