using System.ComponentModel;
using System.Runtime.Versioning;

namespace Aeon.DiskImages.Iso9660;

/// <summary>
/// Stream backed by raw CD sectors.
/// </summary>
[SupportedOSPlatform("windows")]
internal sealed class RawCDReader : Stream
{
    private readonly IntPtr deviceHandle;
    private readonly long diskSize;
    private readonly uint sectorSize;
    private readonly byte[] currentSectorBuffer;
    private uint currentSector;
    private uint currentSectorOffset;
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="RawCDReader"/> class.
    /// </summary>
    /// <param name="drive">The drive to read from.</param>
    public RawCDReader(DriveInfo drive)
    {
        ArgumentNullException.ThrowIfNull(drive);
        if (drive.DriveType != DriveType.CDRom)
            throw new InvalidOperationException("This class may only be used to read from CD-ROM drives.");

        var deviceHandle = NativeMethods.CreateFile(@"\\.\" + drive.Name.TrimEnd('\\'), 0xC0000000, 3, IntPtr.Zero, 3, 0, IntPtr.Zero);
        if (deviceHandle == NativeMethods.INVALID_HANDLE_VALUE)
            throw new Win32Exception();

        this.deviceHandle = deviceHandle;

        var driveGeometry = new IOCTL.CDROM.DISK_GEOMETRY();
        unsafe
        {
            if (!NativeMethods.DeviceIoControl(this.deviceHandle, IOCTL.CDROM.GET_DRIVE_GEOMETRY, IntPtr.Zero, 0, new IntPtr(&driveGeometry), (uint)sizeof(IOCTL.CDROM.DISK_GEOMETRY), out _, IntPtr.Zero))
                throw new Win32Exception();
        }

        this.diskSize = driveGeometry.BytesPerSector * driveGeometry.SectorsPerTrack * driveGeometry.TracksPerCylinder * driveGeometry.Cylinders;
        this.sectorSize = driveGeometry.BytesPerSector;
        this.currentSectorBuffer = new byte[driveGeometry.BytesPerSector];
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="RawCDReader"/> class.
    /// </summary>
    /// <param name="driveName">The name or letter of the drive to read from.</param>
    public RawCDReader(string driveName) : this(new DriveInfo(driveName))
    {
    }

    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => false;
    public override long Length => this.diskSize;
    public override long Position
    {
        get => (long)this.currentSector * this.sectorSize + this.currentSectorOffset;
        set
        {
            uint sector = (uint)(value / this.sectorSize);
            uint offset = (uint)(value % this.sectorSize);

            this.currentSectorOffset = offset;
            if (this.currentSector == sector)
            {
                if (offset == 0)
                    _ = NativeMethods.SetFilePointerEx(this.deviceHandle, sector * this.sectorSize, out _, 0);

                return;
            }

            _ = NativeMethods.SetFilePointerEx(this.deviceHandle, sector * this.sectorSize, out _, 0);
            this.currentSector = sector;
            if (offset != 0)
                ReadCurrentSector();
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
    public override int Read(byte[] buffer, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        int bytesRemaining = count;

        unsafe
        {
            if (this.currentSectorOffset > 0)
            {
                int bufferCopyLength = Math.Min(bytesRemaining, (int)(this.sectorSize - this.currentSectorOffset));
                Array.Copy(this.currentSectorBuffer, this.currentSectorOffset, buffer, offset, bufferCopyLength);
                offset += bufferCopyLength;
                bytesRemaining -= bufferCopyLength;
                this.currentSectorOffset += (uint)bufferCopyLength;
                if (this.currentSectorOffset >= this.sectorSize)
                {
                    this.currentSector++;
                    this.currentSectorOffset = 0;
                }
            }

            if (bytesRemaining == 0)
                return count;

            if (bytesRemaining >= this.sectorSize)
            {
                fixed (byte* ptr = &buffer[offset])
                {
                    if (!NativeMethods.ReadFile(this.deviceHandle, new IntPtr(ptr), (uint)bytesRemaining - ((uint)bytesRemaining % this.sectorSize), out uint bytesReturned, IntPtr.Zero))
                        throw new Win32Exception();

                    offset += (int)bytesReturned;
                    bytesRemaining -= (int)bytesReturned;
                    this.currentSector += bytesReturned / this.sectorSize;
                }
            }

            if (bytesRemaining == 0)
                return count;

            ReadCurrentSector();
            Array.Copy(this.currentSectorBuffer, this.currentSectorOffset, buffer, offset, bytesRemaining);
            this.currentSectorOffset += (uint)bytesRemaining;
        }

        return count;
    }
    public override int ReadByte()
    {
        if (this.currentSectorOffset == 0)
            ReadCurrentSector();

        int value = this.currentSectorBuffer[this.currentSectorOffset++];
        if (this.currentSectorOffset >= this.sectorSize)
        {
            this.currentSectorOffset = 0;
            this.currentSector++;
        }

        return value;
    }
    public override void Flush()
    {
    }
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (!this.disposed)
        {
            this.disposed = true;
            NativeMethods.CloseHandle(this.deviceHandle);
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// Reads the current sector into the sector buffer.
    /// </summary>
    private void ReadCurrentSector()
    {
        unsafe
        {
            fixed (byte* ptr = &this.currentSectorBuffer[0])
            {
                if (!NativeMethods.ReadFile(this.deviceHandle, new IntPtr(ptr), this.sectorSize, out uint bytesReturned, IntPtr.Zero))
                    throw new Win32Exception();
            }
        }
    }
}
