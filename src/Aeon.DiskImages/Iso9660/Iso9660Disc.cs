using System.Text;

namespace Aeon.DiskImages.Iso9660;

/// <summary>
/// Provides access to an ISO-9660 file system.
/// </summary>
public sealed class Iso9660Disc : IDisposable
{
    private readonly Stream rawDisc;
    private readonly Lock rawDiscLock = new();
    private readonly PrimaryVolumeDescriptor pvd;
    private DirectoryEntry? rootEntry;
    private bool initialized;
    private readonly Task initializeTask;

    /// <summary>
    /// Initializes a new instance of the <see cref="Iso9660Disc"/> class.
    /// </summary>
    /// <param name="rawDisc">A stream backed by the raw disc.</param>
    /// <param name="sectorSize">The size of a sector on the disc.</param>
    public Iso9660Disc(Stream rawDisc, int sectorSize = 2048)
    {
        this.rawDisc = rawDisc ?? throw new ArgumentNullException(nameof(rawDisc));
        rawDisc.Position = 16 * sectorSize;

        this.pvd = PrimaryVolumeDescriptor.Read(rawDisc);

        this.initializeTask = Task.Factory.StartNew(Initialize);
    }

    /// <summary>
    /// Gets the primary volume descriptor of the disc.
    /// </summary>
    internal PrimaryVolumeDescriptor PrimaryVolumeDescriptor => this.pvd;

    /// <summary>
    /// Gets the directory entry of a file or folder on the disc.
    /// </summary>
    /// <param name="path">The path of the file or folder.</param>
    /// <returns>Directory entry of the file or folder if found; otherwise null.</returns>
    public DirectoryEntry? GetDirectoryEntry(string path) => this.GetDirectoryEntry(path.Split('\\'));
    /// <summary>
    /// Opens a file on the disc.
    /// </summary>
    /// <param name="entry">Directory entry of the file to open.</param>
    /// <returns></returns>
    public Stream Open(DirectoryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (!this.initialized)
            this.initializeTask.Wait();

        return new BufferedStream(new Iso9660FileStream(this, entry));
    }
    /// <summary>
    /// Reads sectors from the disc into a buffer.
    /// </summary>
    /// <param name="startingSector">Sector to begin reading.</param>
    /// <param name="sectorsToRead">Number of sectors to read.</param>
    /// <param name="buffer">Buffer into which sectors are read.</param>
    public void ReadRaw(int startingSector, int sectorsToRead, Span<byte> buffer)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(startingSector);
        ArgumentOutOfRangeException.ThrowIfNegative(sectorsToRead);

        if (sectorsToRead == 0)
            return;

        if (!this.initialized)
            this.initializeTask.Wait();

        lock (this.rawDiscLock)
        {
            this.rawDisc.Position = startingSector * this.pvd.LogicalBlockSize;
            this.rawDisc.ReadExactly(buffer[..(sectorsToRead * this.pvd.LogicalBlockSize)]);
        }
    }
    public void Dispose() => this.rawDisc.Dispose();

    /// <summary>
    /// Gets the directory entry of a file or folder on the disc.
    /// </summary>
    /// <param name="pathElements">List of path elements of the file or folder.</param>
    /// <returns>Directory entry of the file or folder if found; otherwise null.</returns>
    internal DirectoryEntry? GetDirectoryEntry(IList<string> pathElements)
    {
        if (!this.initialized)
            this.initializeTask.Wait();

        if (pathElements.Count == 0)
            return this.rootEntry;

        IList<DirectoryEntry> container = this.rootEntry!.Children;

        for (int i = 0; i < pathElements.Count - 1; i++)
        {
            var element = pathElements[i];
            var item = container.Where(e => e.Identifier == element).FirstOrDefault();
            if (item == null)
                return null;

            container = item.Children;
        }

        var entry = container.Where(e => e.Identifier == pathElements[pathElements.Count - 1]).FirstOrDefault();
        entry ??= container.Where(e => e.Identifier == pathElements[pathElements.Count - 1] + ";1").FirstOrDefault();

        return entry;
    }

    /// <summary>
    /// Initializes the internal list of directory entries.
    /// </summary>
    private void Initialize()
    {
        var directoriesToRead = new Stack<DirectoryEntry>();
        using var reader = new BinaryReader(rawDisc, Encoding.ASCII, true);

        rawDisc.Position = this.pvd.RootDirectoryEntry.LBALocation * this.pvd.LogicalBlockSize;

        this.rootEntry = DirectoryEntry.Read(reader)!;
        DirectoryEntry.Read(reader); // Skip the parent directory
        var entry = DirectoryEntry.Read(reader);
        while (entry != null)
        {
            this.rootEntry.AddChild(entry);
            if (entry.FileFlags.HasFlag(DirectoryEntryFlags.Directory))
                directoriesToRead.Push(entry);

            entry = DirectoryEntry.Read(reader);
        }

        while (directoriesToRead.Count > 0)
        {
            var containerEntry = directoriesToRead.Pop();
            rawDisc.Position = containerEntry.LBALocation * this.pvd.LogicalBlockSize;
            var current = DirectoryEntry.Read(reader); // Skip this directory
            DirectoryEntry.Read(reader); // Skip the parent directory

            int currentSector = 1;
            int sectors = (int)(current!.DataLength / this.pvd.LogicalBlockSize);
            if ((current.DataLength % this.pvd.LogicalBlockSize) != 0)
                sectors++;

            entry = DirectoryEntry.Read(reader);
            while (entry != null)
            {
                containerEntry.AddChild(entry);
                if (entry.FileFlags.HasFlag(DirectoryEntryFlags.Directory))
                    directoriesToRead.Push(entry);

                entry = DirectoryEntry.Read(reader);
                if (entry == null && currentSector < sectors)
                {
                    rawDisc.Position = (containerEntry.LBALocation + (uint)currentSector) * this.pvd.LogicalBlockSize;
                    currentSector++;
                    entry = DirectoryEntry.Read(reader);
                }
            }
        }

        this.initialized = true;
    }

    /// <summary>
    /// Stream backed by an ISO-9660 file.
    /// </summary>
    private sealed class Iso9660FileStream : Stream
    {
        private readonly Iso9660Disc owner;
        private readonly DirectoryEntry entry;
        private readonly long startPosition;

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

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => this.entry.DataLength;
        public override long Position { get; set; }

        public override int Read(Span<byte> buffer)
        {
            if (buffer.IsEmpty || this.Position >= this.Length)
                return 0;


            int count = Math.Min(buffer.Length, (int)(this.Length - this.Position));
            lock (this.owner.rawDiscLock)
            {
                this.owner.rawDisc.Position = this.startPosition + this.Position;
                count = this.owner.rawDisc.Read(buffer[..count]);
                this.Position += count;
            }

            return count;
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            if (offset < 0 || offset >= buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0 || offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (count == 0 || this.Position >= this.Length)
                return 0;

            count = Math.Min(count, (int)(this.Length - this.Position));
            lock (this.owner.rawDiscLock)
            {
                this.owner.rawDisc.Position = this.startPosition + this.Position;
                this.owner.rawDisc.ReadExactly(buffer, offset, count);
                this.Position += count;
            }

            return count;
        }
        public override int ReadByte()
        {
            if (this.Position >= this.Length)
                return -1;

            lock (this.owner.rawDiscLock)
            {
                this.owner.rawDisc.Position = this.startPosition + this.Position;
                int value = this.owner.rawDisc.ReadByte();
                this.Position++;
                return value;
            }
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    this.Position = offset;
                    break;

                case SeekOrigin.Current:
                    this.Position += offset;
                    break;

                case SeekOrigin.End:
                    this.Position = this.Length + offset;
                    break;
            }

            return this.Position;
        }
        public override void Flush()
        {
        }
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
