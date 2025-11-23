namespace Aeon.Emulator.Dos.VirtualFileSystem;

/// <summary>
/// Describes a track on a CD-ROM.
/// </summary>
/// <param name="Format">Format of the track.</param>
/// <param name="Indexes">Indexes of the track.</param>
/// <param name="PreGap">Track pregap duration.</param>
/// <param name="PostGap">Track postgap duration.</param>
public sealed record class TrackInfo(TrackFormat Format, IReadOnlyList<TrackIndex> Indexes, CDTimeSpan PreGap = default, CDTimeSpan PostGap = default)
{
    /// <summary>
    /// Gets the start offset of the track excluding any pregap.
    /// </summary>
    public CDTimeSpan Offset
    {
        get
        {
            if (this.Indexes.Count == 0)
                return default;

            if (this.Indexes.Count > 1)
            {
                foreach (var i in this.Indexes)
                {
                    if (i.Number == 1)
                        return i.Position;
                }
            }

            return this.Indexes[0].Position;
        }
    }
}
