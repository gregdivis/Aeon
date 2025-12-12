using System.Runtime.InteropServices;

namespace MooParser;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 15)]
public struct CycleState
{
    public PinBitfield0 PinBitfield0;
    public uint AddressLatch;
    public SegmentStatus SegmentStatus;
    public MemoryStatus MemoryStatus;
    public MemoryStatus IOStatus;
    public PinBitfield1 PinBitfield1;
    public ushort DataBus;
    public byte BusStatus;
    public byte TState;
    public byte QueueOpStatus;
    public byte QueueByteRead;
}
