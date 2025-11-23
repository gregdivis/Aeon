namespace Aeon.Emulator;

/// <summary>
/// Segment index as an R/M code.
/// </summary>
public enum SegmentIndex : uint
{
    /// <summary>
    /// The ES segment.
    /// </summary>
    ES,
    /// <summary>
    /// The CS segment.
    /// </summary>
    CS,
    /// <summary>
    /// The SS segment.
    /// </summary>
    SS,
    /// <summary>
    /// The DS segment.
    /// </summary>
    DS,
    /// <summary>
    /// The FS segment.
    /// </summary>
    FS,
    /// <summary>
    /// The GS segment.
    /// </summary>
    GS
}
