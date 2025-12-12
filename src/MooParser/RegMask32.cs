namespace MooParser;

[Flags]
public enum RegMask32 : uint
{
    None = 0,
    CR0 = 1 << 0,
    CR3 = 1 << 1,
    EAX = 1 << 2,
    EBX = 1 << 3,
    ECX = 1 << 4,
    EDX = 1 << 5,
    ESI = 1 << 6,
    EDI = 1 << 7,
    EBP = 1 << 8,
    ESP = 1 << 9,
    CS = 1 << 10,
    DS = 1 << 11,
    ES = 1 << 12,
    FS = 1 << 13,
    GS = 1 << 14,
    SS = 1 << 15,
    EIP = 1 << 16,
    EFlags = 1 << 17,
    DR6 = 1 << 18,
    DR7 = 1 << 19
}
