using System.Runtime.InteropServices;

namespace Aeon.Emulator;

[StructLayout(LayoutKind.Explicit)]
internal struct GeneralPurposeRegisters
{
    [FieldOffset(0)]
    public int EAX;
    [FieldOffset(0)]
    public short AX;
    [FieldOffset(0)]
    public byte AL;
    [FieldOffset(1)]
    public byte AH;

    [FieldOffset(4)]
    public int ECX;
    [FieldOffset(4)]
    public short CX;
    [FieldOffset(4)]
    public byte CL;
    [FieldOffset(5)]
    public byte CH;

    [FieldOffset(8)]
    public int EDX;
    [FieldOffset(8)]
    public short DX;
    [FieldOffset(8)]
    public byte DL;
    [FieldOffset(9)]
    public byte DH;

    [FieldOffset(12)]
    public int EBX;
    [FieldOffset(12)]
    public short BX;
    [FieldOffset(12)]
    public byte BL;
    [FieldOffset(13)]
    public byte BH;

    [FieldOffset(16)]
    public uint ESP;
    [FieldOffset(16)]
    public ushort SP;

    [FieldOffset(20)]
    public uint EBP;
    [FieldOffset(20)]
    public ushort BP;

    [FieldOffset(24)]
    public uint ESI;
    [FieldOffset(24)]
    public ushort SI;

    [FieldOffset(28)]
    public uint EDI;
    [FieldOffset(28)]
    public ushort DI;

    [FieldOffset(32)]
    public uint EIP;
    [FieldOffset(32)]
    public ushort IP;
}
