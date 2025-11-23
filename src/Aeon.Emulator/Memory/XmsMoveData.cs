using System.Runtime.InteropServices;

namespace Aeon.Emulator.Memory;

/// <summary>
/// In-memory structure with information about an XMS move request.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct XmsMoveData
{
    /// <summary>
    /// Number of bytes to move.
    /// </summary>
    public uint Length;
    /// <summary>
    /// Handle of source block; zero if moving from segment:offset pair.
    /// </summary>
    public ushort SourceHandle;
    /// <summary>
    /// Source offset as a 32-bit value or a segment:offset pair.
    /// </summary>
    public uint SourceOffset;
    /// <summary>
    /// Handle of destination block; zero if moving to segment:offset pair.
    /// </summary>
    public ushort DestHandle;
    /// <summary>
    /// Destination offset as a 32-bit value or a segment:offset pair.
    /// </summary>
    public uint DestOffset;

    /// <summary>
    /// Gets the source address as a segment:offset value.
    /// </summary>
    public readonly RealModeAddress SourceAddress => new((ushort)(this.SourceOffset >> 16), (ushort)this.SourceOffset);
    /// <summary>
    /// Gets the destination address as a segment:offset value.
    /// </summary>
    public readonly RealModeAddress DestAddress => new((ushort)(this.DestOffset >> 16), (ushort)this.DestOffset);
}
