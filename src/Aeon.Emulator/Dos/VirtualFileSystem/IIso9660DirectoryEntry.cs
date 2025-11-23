namespace Aeon.Emulator.Dos.VirtualFileSystem;

/// <summary>
/// Provides properies for accessing additional metadata for files in an ISO9660 file system.
/// </summary>
public interface IIso9660DirectoryEntry
{
    /// <summary>
    /// Gets the XAR length for the entry.
    /// </summary>
    byte ExtendedAttributeRecordLength { get; }
    /// <summary>
    /// Gets the sector of the entry's LBA.
    /// </summary>
    uint LBALocation { get; }
    /// <summary>
    /// Gets the length of the entry in bytes.
    /// </summary>
    uint DataLength { get; }
    /// <summary>
    /// Gets the date when the entry was created.
    /// </summary>
    DateTimeOffset? RecordingDate { get; }
    /// <summary>
    /// Gets the entry's flags.
    /// </summary>
    byte FileFlags { get; }
    /// <summary>
    /// Gets the entry's interleaved unit size.
    /// </summary>
    byte InterleavedUnitSize { get; }
    /// <summary>
    /// Gets the entry's interleaved gap size.
    /// </summary>
    byte InterleavedGapSize { get; }
    /// <summary>
    /// Gets the volume sequence number.
    /// </summary>
    ushort VolumeSequenceNumber { get; }
    /// <summary>
    /// Gets the entry's identifier.
    /// </summary>
    string Identifier { get; }
}
