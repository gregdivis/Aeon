using System;

namespace Aeon.Emulator
{
    /// <summary>
    /// Describes additional properties of a DOS file.
    /// </summary>
    [Flags]
    public enum VirtualFileAttributes
    {
        /// <summary>
        /// The file has no special properties.
        /// </summary>
        Default = 0,
        /// <summary>
        /// The file is read-only.
        /// </summary>
        ReadOnly = 0x01,
        /// <summary>
        /// The file is hidden.
        /// </summary>
        Hidden = 0x02,
        /// <summary>
        /// The file is a system file.
        /// </summary>
        System = 0x04,
        /// <summary>
        /// The file is a volume label.
        /// </summary>
        VolumeLabel = 0x08,
        /// <summary>
        /// The file is a directory.
        /// </summary>
        Directory = 0x10,
        /// <summary>
        /// The file has been archived.
        /// </summary>
        Archived = 0x20
    }
}
