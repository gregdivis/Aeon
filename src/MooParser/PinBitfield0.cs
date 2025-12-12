namespace MooParser;

[Flags]
public enum PinBitfield0 : byte
{
    Clear = 0,
    Ale = 1 << 0,
    Bhe = 1 << 1,
    Ready = 1 << 2,
    Lock = 1 << 3
}
