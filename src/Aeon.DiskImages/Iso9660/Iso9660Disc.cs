using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Aeon.DiskImages.Iso9660
{
    /// <summary>
    /// Provides access to an ISO-9660 file system.
    /// </summary>
    public sealed class Iso9660Disc : IDisposable
    {
        #region Private Fields
        private readonly Stream rawDisc;
        private readonly object rawDiscLock = new object();
        private PrimaryVolumeDescriptor pvd;
        private DirectoryEntry rootEntry;
        private bool initialized;
        private Task initializeTask;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Iso9660Disc"/> class.
        /// </summary>
        /// <param name="rawDisc">A stream backed by the raw disc.</param>
        /// <param name="sectorSize">The size of a sector on the disc.</param>
        public Iso9660Disc(Stream rawDisc, int sectorSize)
        {
            if(rawDisc == null)
                throw new ArgumentNullException("rawDisc");

            this.rawDisc = rawDisc;
            rawDisc.Position = 16 * sectorSize;

            this.pvd = PrimaryVolumeDescriptor.Read(rawDisc);

            this.initializeTask = Task.Factory.StartNew(Initialize);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Iso9660Disc"/> class.
        /// </summary>
        /// <param name="rawDisc">A stream backed by the raw disc.</param>
        public Iso9660Disc(Stream rawDisc)
            : this(rawDisc, 2048)
        {
        }
        #endregion

        #region Internal Properties
        /// <summary>
        /// Gets the primary volume descriptor of the disc.
        /// </summary>
        internal PrimaryVolumeDescriptor PrimaryVolumeDescriptor
        {
            get { return this.pvd; }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets the directory entry of a file or folder on the disc.
        /// </summary>
        /// <param name="path">The path of the file or folder.</param>
        /// <returns>Directory entry of the file or folder if found; otherwise null.</returns>
        public DirectoryEntry GetDirectoryEntry(string path)
        {
            return GetDirectoryEntry(path.Split('\\'));
        }
        /// <summary>
        /// Opens a file on the disc.
        /// </summary>
        /// <param name="entry">Directory entry of the file to open.</param>
        /// <returns></returns>
        public Stream Open(DirectoryEntry entry)
        {
            if(entry == null)
                throw new ArgumentNullException("entry");

            if(!this.initialized)
                this.initializeTask.Wait();

            return new BufferedStream(new Iso9660FileStream(this, entry));
        }
        /// <summary>
        /// Reads sectors from the disc into a buffer.
        /// </summary>
        /// <param name="startingSector">Sector to begin reading.</param>
        /// <param name="sectorsToRead">Number of sectors to read.</param>
        /// <param name="buffer">Buffer into which sectors are read.</param>
        /// <param name="offset">Offset in <paramref name="buffer"/> to start writing.</param>
        public void ReadRaw(int startingSector, int sectorsToRead, byte[] buffer, int offset)
        {
            if(buffer == null)
                throw new ArgumentNullException("buffer");
            if(offset < 0 || offset >= buffer.Length)
                throw new ArgumentOutOfRangeException("offset");
            if(startingSector < 0)
                throw new ArgumentOutOfRangeException("startingSector");
            if(sectorsToRead < 0)
                throw new ArgumentOutOfRangeException("sectorsToRead");

            if(sectorsToRead == 0)
                return;

            if(!this.initialized)
                this.initializeTask.Wait();

            lock(this.rawDiscLock)
            {
                this.rawDisc.Position = startingSector * this.pvd.LogicalBlockSize;
                this.rawDisc.Read(buffer, offset, sectorsToRead * this.pvd.LogicalBlockSize);
            }
        }
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.rawDisc.Dispose();
        }
        #endregion

        #region Internal Methods
        /// <summary>
        /// Gets the directory entry of a file or folder on the disc.
        /// </summary>
        /// <param name="pathElements">List of path elements of the file or folder.</param>
        /// <returns>Directory entry of the file or folder if found; otherwise null.</returns>
        internal DirectoryEntry GetDirectoryEntry(IList<string> pathElements)
        {
            if(!this.initialized)
                this.initializeTask.Wait();

            if(pathElements.Count == 0)
                return this.rootEntry;

            IList<DirectoryEntry> container = this.rootEntry.Children;

            for(int i = 0; i < pathElements.Count - 1; i++)
            {
                var element = pathElements[i];
                var item = container.Where(e => e.Identifier == element).FirstOrDefault();
                if(item == null)
                    return null;

                container = item.Children;
            }

            var entry = container.Where(e => e.Identifier == pathElements[pathElements.Count - 1]).FirstOrDefault();
            if(entry == null)
                entry = container.Where(e => e.Identifier == pathElements[pathElements.Count - 1] + ";1").FirstOrDefault();

            return entry;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Initializes the internal list of directory entries.
        /// </summary>
        private void Initialize()
        {
            var directoriesToRead = new Stack<DirectoryEntry>();
            var reader = new BinaryReader(rawDisc);

            rawDisc.Position = this.pvd.RootDirectoryEntry.LBALocation * this.pvd.LogicalBlockSize;

            this.rootEntry = DirectoryEntry.Read(reader);
            DirectoryEntry.Read(reader); // Skip the parent directory
            var entry = DirectoryEntry.Read(reader);
            while(entry != null)
            {
                this.rootEntry.AddChild(entry);
                if((entry.FileFlags & DirectoryEntryFlags.Directory) != 0)
                    directoriesToRead.Push(entry);

                entry = DirectoryEntry.Read(reader);
            }

            while(directoriesToRead.Count > 0)
            {
                var containerEntry = directoriesToRead.Pop();
                rawDisc.Position = containerEntry.LBALocation * this.pvd.LogicalBlockSize;
                var current = DirectoryEntry.Read(reader); // Skip this directory
                DirectoryEntry.Read(reader); // Skip the parent directory

                int currentSector = 1;
                int sectors = (int)(current.DataLength / this.pvd.LogicalBlockSize);
                if((current.DataLength % this.pvd.LogicalBlockSize) != 0)
                    sectors++;

                entry = DirectoryEntry.Read(reader);
                while(entry != null)
                {
                    containerEntry.AddChild(entry);
                    if((entry.FileFlags & DirectoryEntryFlags.Directory) != 0)
                        directoriesToRead.Push(entry);

                    entry = DirectoryEntry.Read(reader);
                    if(entry == null && currentSector < sectors)
                    {
                        rawDisc.Position = (containerEntry.LBALocation + (uint)currentSector) * this.pvd.LogicalBlockSize;
                        currentSector++;
                        entry = DirectoryEntry.Read(reader);
                    }
                }
            }

            this.initialized = true;
        }
        #endregion

        #region Private Iso9660FileStream Class
        /// <summary>
        /// Stream backed by an ISO-9660 file.
        /// </summary>
        private sealed class Iso9660FileStream : Stream
        {
            private readonly Iso9660Disc owner;
            private readonly DirectoryEntry entry;
            private readonly long startPosition;
            private long position;

            /// <summary>
            /// Initializes a new instance of the <see cref="Iso9660FileStream"/> class.
            /// </summary>
            /// <param name="owner">The disc which contains this file.</param>
            /// <param name="entry">The entry of this file.</param>
            public Iso9660FileStream(Iso9660Disc owner, DirectoryEntry entry)
            {
                this.owner = owner;
                this.entry = entry;
                this.startPosition = entry.LBALocation * owner.pvd.LogicalBlockSize;
            }

            /// <summary>
            /// Gets a value indicating whether the current stream supports reading.
            /// </summary>
            public override bool CanRead
            {
                get { return true; }
            }
            /// <summary>
            /// Gets a value indicating whether the current stream supports seeking.
            /// </summary>
            public override bool CanSeek
            {
                get { return true; }
            }
            /// <summary>
            /// Gets a value indicating whether the current stream supports writing.
            /// </summary>
            public override bool CanWrite
            {
                get { return false; }
            }
            /// <summary>
            /// Gets the length in bytes of the stream.
            /// </summary>
            public override long Length
            {
                get { return this.entry.DataLength; }
            }
            /// <summary>
            /// Gets or sets the position within the current stream.
            /// </summary>
            public override long Position
            {
                get { return this.position; }
                set { this.position = value; }
            }

            /// <summary>
            /// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
            /// </summary>
            /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset"/> and (<paramref name="offset"/> + <paramref name="count"/> - 1) replaced by the bytes read from the current source.</param>
            /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin storing the data read from the current stream.</param>
            /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
            /// <returns>
            /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
            /// </returns>
            public override int Read(byte[] buffer, int offset, int count)
            {
                if(buffer == null)
                    throw new ArgumentNullException("buffer");
                if(offset < 0 || offset >= buffer.Length)
                    throw new ArgumentOutOfRangeException("offset");
                if(count < 0 || offset + count > buffer.Length)
                    throw new ArgumentOutOfRangeException("count");

                if(count == 0 || this.position >= this.Length)
                    return 0;

                count = Math.Min(count, (int)(this.Length - this.Position));
                lock(this.owner.rawDiscLock)
                {
                    this.owner.rawDisc.Position = this.startPosition + this.position;
                    this.owner.rawDisc.Read(buffer, offset, count);
                    this.position += count;
                }

                return count;
            }
            /// <summary>
            /// Reads a byte from the stream and advances the position within the stream by one byte, or returns -1 if at the end of the stream.
            /// </summary>
            /// <returns>
            /// The unsigned byte cast to an Int32, or -1 if at the end of the stream.
            /// </returns>
            public override int ReadByte()
            {
                if(this.position >= this.Length)
                    return -1;

                lock(this.owner.rawDiscLock)
                {
                    this.owner.rawDisc.Position = this.startPosition + this.position;
                    int value = this.owner.rawDisc.ReadByte();
                    this.position++;
                    return value;
                }
            }
            /// <summary>
            /// Sets the position within the current stream.
            /// </summary>
            /// <param name="offset">A byte offset relative to the <paramref name="origin"/> parameter.</param>
            /// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin"/> indicating the reference point used to obtain the new position.</param>
            /// <returns>
            /// The new position within the current stream.
            /// </returns>
            public override long Seek(long offset, SeekOrigin origin)
            {
                switch(origin)
                {
                case SeekOrigin.Begin:
                    this.position = offset;
                    break;

                case SeekOrigin.Current:
                    this.position += offset;
                    break;

                case SeekOrigin.End:
                    this.position = this.Length + offset;
                    break;
                }

                return this.position;
            }
            /// <summary>
            /// When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.
            /// </summary>
            public override void Flush()
            {
            }
            /// <summary>
            /// Not supported.
            /// </summary>
            /// <param name="value">Not supported.</param>
            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }
            /// <summary>
            /// Not supported.
            /// </summary>
            /// <param name="buffer">Not supported.</param>
            /// <param name="offset">Not supported.</param>
            /// <param name="count">Not supported.</param>
            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }
        }
        #endregion
    }
}
