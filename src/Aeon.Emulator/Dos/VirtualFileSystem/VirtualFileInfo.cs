using System;

namespace Aeon.Emulator
{
    /// <summary>
    /// Describes a file in a virtual directory.
    /// </summary>
    public class VirtualFileInfo : IEquatable<VirtualFileInfo>
    {
        private static uint DosAttributeMask = 0x3F;

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualFileInfo"/> class.
        /// </summary>
        /// <param name="name">Name of the file.</param>
        /// <param name="attributes">DOS attributes of the file.</param>
        /// <param name="modifyDate">Date and time when file was last modified.</param>
        /// <param name="length">Size of the file in bytes.</param>
        public VirtualFileInfo(string name, VirtualFileAttributes attributes, DateTime modifyDate, long length)
        {
            this.Name = name;
            this.Attributes = attributes;
            this.ModifyDate = modifyDate;
            this.Length = length;
        }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Gets the DOS attributes of the file.
        /// </summary>
        public VirtualFileAttributes Attributes { get; }
        /// <summary>
        /// Gets the date and time when the file was last modified.
        /// </summary>
        public DateTime ModifyDate { get; }
        /// <summary>
        /// Gets the size of the file in bytes.
        /// </summary>
        public long Length { get; }

        /// <summary>
        /// Gets the modify date in the DOS date format.
        /// </summary>
        internal ushort DosModifyDate
        {
            get
            {
                DateTime localDate = this.ModifyDate.ToLocalTime();
                uint year = (uint)((localDate.Year - 1980) << 9);
                uint month = (uint)(localDate.Month << 5);
                uint day = (uint)localDate.Day;
                return (ushort)(year | month | day);
            }
        }
        /// <summary>
        /// Gets the modify time in the DOS time format.
        /// </summary>
        internal ushort DosModifyTime
        {
            get
            {
                TimeSpan localTime = this.ModifyDate.ToLocalTime().TimeOfDay;
                uint hours = (uint)(localTime.Hours << 11);
                uint minutes = (uint)(localTime.Minutes << 5);
                uint seconds = (uint)(localTime.Seconds / 2);
                return (ushort)(hours | minutes | seconds);
            }
        }
        /// <summary>
        /// Gets the length of a file constrained to a 32-bit number.
        /// </summary>
        internal uint DosLength
        {
            get
            {
                if (this.Length <= uint.MaxValue)
                    return (uint)this.Length;
                else
                    return uint.MaxValue;
            }
        }
        /// <summary>
        /// Gets the file attributes supported by DOS.
        /// </summary>
        internal byte DosAttributes
        {
            get
            {
                return (byte)((uint)this.Attributes & DosAttributeMask);
            }
        }
        /// <summary>
        /// Gets or sets the index of the device which contains the file.
        /// (A: = 0, B: = 1, ...)
        /// </summary>
        internal int DeviceIndex { get; set; }

        /// <summary>
        /// Gets a string containing the name of the file.
        /// </summary>
        /// <returns>String containing the name of the file.</returns>
        public override string ToString() => this.Name;
        /// <summary>
        /// Tests for equality with another object.
        /// </summary>
        /// <param name="obj">Other object to test.</param>
        /// <returns>True if objects are equal; otherwise false.</returns>
        public override bool Equals(object obj) => this.Equals(obj as VirtualFileInfo);
        /// <summary>
        /// Tests for equality with another instance.
        /// </summary>
        /// <param name="other">Other instance to test.</param>
        /// <returns>True if files have the same name; otherwise false.</returns>
        public bool Equals(VirtualFileInfo other)
        {
            if (ReferenceEquals(other, null))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            if (this.Name != null && other.Name != null)
                return string.Compare(this.Name, other.Name, true) == 0;
            else
                return this.Name == other.Name;
        }
        /// <summary>
        /// Gets a hash code based on the file name.
        /// </summary>
        /// <returns>Hash code based on the file name.</returns>
        public override int GetHashCode()
        {
            if (this.Name != null)
                return this.Name.GetHashCode();
            else
                return 0;
        }
    }

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
