using System;
using System.IO;
using System.Text;

namespace Aeon.DiskImages.Iso9660
{
    /// <summary>
    /// Represents an ISO-9660 primary volume descriptor.
    /// </summary>
    internal sealed class PrimaryVolumeDescriptor
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="PrimaryVolumeDescriptor"/> class from being created.
        /// </summary>
        private PrimaryVolumeDescriptor()
        {
        }

        public string SystemIdentifier { get; private set; }
        public string VolumeIdentifier { get; private set; }
        public uint VolumeSpaceSize { get; private set; }
        public ushort VolumeSetSize { get; private set; }
        public ushort VolumeSequenceNumber { get; private set; }
        public ushort LogicalBlockSize { get; private set; }
        public uint PathTableSize { get; private set; }
        public uint PathTableLocation { get; private set; }
        public uint OptionalPathTableLocation { get; private set; }
        public DirectoryEntry RootDirectoryEntry { get; private set; }
        public string VolumeSetIdentifier { get; private set; }
        public string PublisherIdentifier { get; private set; }
        public string DataPreparerIdentifier { get; private set; }
        public string ApplicationIdentifier { get; private set; }
        public string CopyrightFileIdentifier { get; private set; }
        public string AbstractFileIdentifier { get; private set; }
        public string BibliographicFileIdentifier { get; private set; }
        public DateTimeOffset? VolumeCreationDate { get; private set; }
        public DateTimeOffset? VolumeModificationDate { get; private set; }
        public DateTimeOffset? VolumeExpirationDate { get; private set; }
        public DateTimeOffset? VolumeEffectiveDate { get; private set; }

        /// <summary>
        /// Reads a primary volume descriptor from a stream.
        /// </summary>
        /// <param name="stream">Stream to read the primary volume descriptor from.</param>
        /// <returns>Primary volume descriptor read from the stream.</returns>
        public static PrimaryVolumeDescriptor Read(Stream stream)
        {
            var reader = new BinaryReader(stream);

            if(reader.ReadByte() != 0x01)
                return null;
            if(ReadAsciiString(reader, 5) != "CD001")
                throw new InvalidDataException("Not a primary volume descriptor.");

            reader.ReadByte(); // Version: 0x01
            reader.ReadByte(); // Unused

            var pvd = new PrimaryVolumeDescriptor();
            pvd.SystemIdentifier = ReadAsciiString(reader, 32);
            pvd.VolumeIdentifier = ReadAsciiString(reader, 32);
            stream.Seek(8, SeekOrigin.Current);
            
            pvd.VolumeSpaceSize = reader.ReadUInt32();
            stream.Seek(36, SeekOrigin.Current);

            pvd.VolumeSetSize = reader.ReadUInt16();
            stream.Seek(2, SeekOrigin.Current);

            pvd.VolumeSequenceNumber = reader.ReadUInt16();
            stream.Seek(2, SeekOrigin.Current);

            pvd.LogicalBlockSize = reader.ReadUInt16();
            stream.Seek(2, SeekOrigin.Current);

            pvd.PathTableSize = reader.ReadUInt32();
            stream.Seek(4, SeekOrigin.Current);

            pvd.PathTableLocation = reader.ReadUInt32();
            pvd.OptionalPathTableLocation = reader.ReadUInt32();
            stream.Seek(8, SeekOrigin.Current);

            pvd.RootDirectoryEntry = DirectoryEntry.Read(reader);
            pvd.VolumeSetIdentifier = ReadAsciiString(reader, 128);
            pvd.PublisherIdentifier = ReadAsciiString(reader, 128);
            pvd.DataPreparerIdentifier = ReadAsciiString(reader, 128);
            pvd.ApplicationIdentifier = ReadAsciiString(reader, 128);
            pvd.CopyrightFileIdentifier = ReadAsciiString(reader, 38);
            pvd.AbstractFileIdentifier = ReadAsciiString(reader, 36);
            pvd.BibliographicFileIdentifier = ReadAsciiString(reader, 37);

            pvd.VolumeCreationDate = ReadDate(reader);
            pvd.VolumeModificationDate = ReadDate(reader);
            pvd.VolumeExpirationDate = ReadDate(reader);
            pvd.VolumeEffectiveDate = ReadDate(reader);

            return pvd;
        }

        private static string ReadAsciiString(BinaryReader reader, int length)
        {
            var bytes = reader.ReadBytes(length);
            return Encoding.ASCII.GetString(bytes).TrimEnd('\0');
        }
        private static DateTimeOffset? ReadDate(BinaryReader reader)
        {
            int year = ParseZero(Encoding.ASCII.GetString(reader.ReadBytes(4)));
            int month = ParseZero(Encoding.ASCII.GetString(reader.ReadBytes(2)));
            int day = ParseZero(Encoding.ASCII.GetString(reader.ReadBytes(2)));
            int hour = ParseZero(Encoding.ASCII.GetString(reader.ReadBytes(2)));
            int minute = ParseZero(Encoding.ASCII.GetString(reader.ReadBytes(2)));
            int second = ParseZero(Encoding.ASCII.GetString(reader.ReadBytes(2)));
            int hundreths = ParseZero(Encoding.ASCII.GetString(reader.ReadBytes(2)));
            int offset = reader.ReadSByte();

            if(year == 0)
                return null;

            return new DateTimeOffset(year, month, day, hour, minute, second, hundreths * 10, TimeSpan.FromMinutes(offset * 15));
        }
        private static int ParseZero(string s)
        {
            int value;
            int.TryParse(s, out value);
            return value;
        }
    }
}
