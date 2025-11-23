namespace Aeon.Emulator.Memory;

/// <summary>
/// Represents a real-mode memory address.
/// </summary>
/// <param name="Segment">The segment value.</param>
/// <param name="Offset">The offset value.</param>
public readonly record struct RealModeAddress(ushort Segment, ushort Offset)
{
    public override string ToString() => $"{this.Segment:X4}:{this.Offset:X4}";
}
