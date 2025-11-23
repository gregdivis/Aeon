namespace Aeon.Emulator;

/// <summary>
/// Represents the possible segment override modes.
/// </summary>
public enum SegmentRegister : byte
{
    /// <summary>
    /// No segment override is set.
    /// </summary>
    Default,
    /// <summary>
    /// The current segment is CS.
    /// </summary>
    CS,
    /// <summary>
    /// The current segment is SS.
    /// </summary>
    SS,
    /// <summary>
    /// The current segment is DS.
    /// </summary>
    DS,
    /// <summary>
    /// The current segment is ES.
    /// </summary>
    ES,
    /// <summary>
    /// The current segment is FS.
    /// </summary>
    FS,
    /// <summary>
    /// The current segment is GS.
    /// </summary>
    GS
}
