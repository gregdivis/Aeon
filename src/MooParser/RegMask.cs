namespace MooParser;

[Flags]
public enum RegMask : ushort
{
    None = 0,
    AX = 1 << 0,
    BX = 1 << 1,
    CX = 1 << 2,
    DX = 1 << 3,
    CS = 1 << 4,
    SS = 1 << 5,
    DS = 1 << 6,
    ES = 1 << 7,
    SP = 1 << 8,
    BP = 1 << 9,
    SI = 1 << 10,
    DI = 1 << 11,
    IP = 1 << 12,
    Flags = 1 << 13
}
