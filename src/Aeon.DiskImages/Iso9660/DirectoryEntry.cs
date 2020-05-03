using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Aeon.Emulator;
using Aeon.Emulator.Dos.VirtualFileSystem;

namespace Aeon.DiskImages.Iso9660
{
    /// <summary>
    /// Represents an ISO-9660 directory entry.
    /// </summary>
    public sealed class DirectoryEntry : VirtualFileInfo, IIso9660DirectoryEntry
    {
        private static readonly DirectoryEntry[] EmptyCollection = new DirectoryEntry[0];
        private List<DirectoryEntry> children;

        /// <summary>
        /// Prevents a default instance of the <see cref="DirectoryEntry"/> class from being created.
        /// </summary>
        /// <param name="identifier">The entry identifier.</param>
        /// <param name="flags">The entry flags.</param>
        /// <param name="recordingDate">The entry recording date.</param>
        /// <param name="length">The entry length.</param>
        private DirectoryEntry(string identifier, DirectoryEntryFlags flags, DateTimeOffset? recordingDate, uint length)
            : base(GetFileName(identifier), GetAttributes(flags), GetModifyDate(recordingDate), length)
        {
            this.Identifier = identifier;
            this.FileFlags = flags;
            this.RecordingDate = recordingDate;
            this.DataLength = length;
        }

        /// <summary>
        /// Gets the XAR length of the entry.
        /// </summary>
        public byte ExtendedAttributeRecordLength { get; private set; }
        /// <summary>
        /// Gets the LBA location of the entry in sectors.
        /// </summary>
        public uint LBALocation { get; private set; }
        /// <summary>
        /// Gets the data length of the entry in bytes.
        /// </summary>
        public uint DataLength { get; private set; }
        /// <summary>
        /// Gets the recording date of the entry.
        /// </summary>
        public DateTimeOffset? RecordingDate { get; private set; }
        /// <summary>
        /// Gets the file flags of the entry.
        /// </summary>
        public DirectoryEntryFlags FileFlags { get; private set; }
        /// <summary>
        /// Gets the entry's flags.
        /// </summary>
        byte IIso9660DirectoryEntry.FileFlags
        {
            get { return (byte)this.FileFlags; }
        }
        /// <summary>
        /// Gets the interleaved unit size of the entry.
        /// </summary>
        public byte InterleavedUnitSize { get; private set; }
        /// <summary>
        /// Gets the interleaved gap size of the entry.
        /// </summary>
        public byte InterleavedGapSize { get; private set; }
        /// <summary>
        /// Gets the volume sequence number.
        /// </summary>
        public ushort VolumeSequenceNumber { get; private set; }
        /// <summary>
        /// Gets the file identifier.
        /// </summary>
        public string Identifier { get; private set; }
        /// <summary>
        /// Gets the children of the entry.
        /// </summary>
        public IList<DirectoryEntry> Children
        {
            get { return this.children ?? (IList<DirectoryEntry>)EmptyCollection; }
        }

        /// <summary>
        /// Reads an entry from a stream.
        /// </summary>
        /// <param name="reader">Stream to read entry from.</param>
        /// <returns>Entry read from the stream.</returns>
        internal static DirectoryEntry Read(BinaryReader reader)
        {
            try
            {
                int length = reader.ReadByte();
                if(length < 34)
                    return null;

                byte xarLength = reader.ReadByte();
                uint lbaLocation = reader.ReadUInt32();
                reader.BaseStream.Seek(4, SeekOrigin.Current);
                uint dataLength = reader.ReadUInt32();
                reader.BaseStream.Seek(4, SeekOrigin.Current);

                int year = reader.ReadByte();
                int month = reader.ReadByte();
                int day = reader.ReadByte();
                int hour = reader.ReadByte();
                int minute = reader.ReadByte();
                int second = reader.ReadByte();
                int offset = reader.ReadSByte();
                DateTimeOffset? recordingDate = null;
                if(year != 0)
                    recordingDate = new DateTimeOffset(1900 + year, month, day, hour, minute, second, TimeSpan.FromMinutes(offset * 15));

                var fileFlags = (DirectoryEntryFlags)reader.ReadByte();
                byte interleavedUnitSize = reader.ReadByte();
                byte interleavedGapSize = reader.ReadByte();
                ushort volumeSequenceNumber = reader.ReadUInt16();
                reader.BaseStream.Seek(2, SeekOrigin.Current);

                string identifier;
                int nameLength = reader.ReadByte();
                if(nameLength > 0)
                {
                    identifier = Encoding.ASCII.GetString(reader.ReadBytes(nameLength)).Trim('\0');
                    if((nameLength % 2) == 0)
                        reader.ReadByte();
                }
                else
                    identifier = string.Empty;

                return new DirectoryEntry(identifier, fileFlags, recordingDate, dataLength)
                {
                    ExtendedAttributeRecordLength = xarLength,
                    LBALocation = lbaLocation,
                    InterleavedUnitSize = interleavedUnitSize,
                    InterleavedGapSize = interleavedGapSize,
                    VolumeSequenceNumber = volumeSequenceNumber
                };
            }
            catch(EndOfStreamException)
            {
                return null;
            }
        }

        /// <summary>
        /// Adds a child entry to this entry.
        /// </summary>
        /// <param name="entry">Child entry to add.</param>
        internal void AddChild(DirectoryEntry entry)
        {
            if(this.children == null)
                this.children = new List<DirectoryEntry>();

            this.children.Add(entry);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Identifier;
        }

        /// <summary>
        /// Returns VirtualFileAttributes based on DirectoryEntryFlags.
        /// </summary>
        /// <param name="flags">Directory entry flags to convert.</param>
        /// <returns>Virtual file attributes based on the specified directory entry flags.</returns>
        private static VirtualFileAttributes GetAttributes(DirectoryEntryFlags flags)
        {
            var attributes = VirtualFileAttributes.Default;

            if((flags & DirectoryEntryFlags.Hidden) != 0)
                attributes |= VirtualFileAttributes.Hidden;
            if((flags & DirectoryEntryFlags.Directory) != 0)
                attributes |= VirtualFileAttributes.Directory;

            return attributes;
        }
        /// <summary>
        /// Returns a local DateTime based on a recording date.
        /// </summary>
        /// <param name="recordingDate">Recording date to convert.</param>
        /// <returns>Local DateTime based on the specified recording date.</returns>
        private static DateTime GetModifyDate(DateTimeOffset? recordingDate)
        {
            if(recordingDate == null)
                return new DateTime(1990, 1, 1);

            return recordingDate.Value.ToLocalTime().DateTime;
        }
        /// <summary>
        /// Returns a DOS file name based on an ISO-9660 identifier.
        /// </summary>
        /// <param name="identifier">ISO-9660 identifier to convert.</param>
        /// <returns>DOS file name based on the specified ISO-9660 identifier.</returns>
        private static string GetFileName(string identifier)
        {
            if(identifier.EndsWith(";1"))
                return identifier.Substring(0, identifier.Length - 2);

            return identifier;
        }
    }

    /// <summary>
    /// ISO-9660 directory entry flags.
    /// </summary>
    [Flags]
    public enum DirectoryEntryFlags : byte
    {
        /// <summary>
        /// No flags are set.
        /// </summary>
        Clear = 0,
        /// <summary>
        /// The entry is hidden.
        /// </summary>
        Hidden = 1 << 0,
        /// <summary>
        /// The entry is a directory.
        /// </summary>
        Directory = 1 << 1,
        /// <summary>
        /// The entry is associated.
        /// </summary>
        Associated = 1 << 2,
        /// <summary>
        /// The entry's format is in its XAR.
        /// </summary>
        FormatInXAR = 1 << 3,
        /// <summary>
        /// The entry has group permissions.
        /// </summary>
        GroupPermissions = 1 << 4,
        /// <summary>
        /// The entry is spanned.
        /// </summary>
        Span = 1 << 7
    }
}
