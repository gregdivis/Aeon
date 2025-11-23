using System.Runtime.InteropServices;

namespace Aeon.Emulator;

[StructLayout(LayoutKind.Explicit)]
internal struct TaskStateSegment32
{
    [FieldOffset(0)]
    public ushort LINK;
    [FieldOffset(4)]
    public uint ESP0;
    [FieldOffset(8)]
    public ushort SS0;
    [FieldOffset(12)]
    public uint ESP1;
    [FieldOffset(16)]
    public ushort SS1;
    [FieldOffset(20)]
    public uint ESP2;
    [FieldOffset(24)]
    public ushort SS2;
    [FieldOffset(28)]
    public uint CR3;
    [FieldOffset(32)]
    public uint EIP;
    [FieldOffset(36)]
    public EFlags EFLAGS;
    [FieldOffset(40)]
    public uint EAX;
    [FieldOffset(44)]
    public uint ECX;
    [FieldOffset(48)]
    public uint EDX;
    [FieldOffset(52)]
    public uint EBX;
    [FieldOffset(56)]
    public uint ESP;
    [FieldOffset(60)]
    public uint EBP;
    [FieldOffset(64)]
    public uint ESI;
    [FieldOffset(68)]
    public uint EDI;
    [FieldOffset(72)]
    public ushort ES;
    [FieldOffset(76)]
    public ushort CS;
    [FieldOffset(80)]
    public ushort SS;
    [FieldOffset(84)]
    public ushort DS;
    [FieldOffset(88)]
    public ushort FS;
    [FieldOffset(92)]
    public ushort GS;
    [FieldOffset(96)]
    public ushort LDTR;
}
