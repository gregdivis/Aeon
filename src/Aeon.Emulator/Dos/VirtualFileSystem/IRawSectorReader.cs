using System;

namespace Aeon.Emulator.Dos.VirtualFileSystem
{
    /// <summary>
    /// Describes a device which allows sector-level reading.
    /// </summary>
    public interface IRawSectorReader
    {
        /// <summary>
        /// The size of each sector in bytes.
        /// </summary>
        int SectorSize { get; }

        /// <summary>
        /// Reads sectors from the device into a buffer.
        /// </summary>
        /// <param name="startingSector">Sector to begin reading.</param>
        /// <param name="sectorsToRead">Number of sectors to read.</param>
        /// <param name="buffer">Buffer into which sectors are read.</param>
        void ReadSectors(int startingSector, int sectorsToRead, Span<byte> buffer);
    }
}
