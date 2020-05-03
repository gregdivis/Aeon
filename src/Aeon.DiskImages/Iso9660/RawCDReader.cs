using System;
using System.ComponentModel;
using System.IO;

namespace Aeon.DiskImages.Iso9660
{
    /// <summary>
    /// Stream backed by raw CD sectors.
    /// </summary>
    internal sealed class RawCDReader : Stream
    {
        #region Private Fields
        private readonly IntPtr deviceHandle;
        private readonly long diskSize;
        private readonly uint sectorSize;
        private readonly byte[] currentSectorBuffer;
        private uint currentSector;
        private uint currentSectorOffset;
        private bool disposed;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="RawCDReader"/> class.
        /// </summary>
        /// <param name="drive">The drive to read from.</param>
        public RawCDReader(DriveInfo drive)
        {
            if(drive == null)
                throw new ArgumentNullException("drive");
            if(drive.DriveType != DriveType.CDRom)
                throw new InvalidOperationException("This class may only be used to read from CD-ROM drives.");

            var deviceHandle = NativeMethods.CreateFile(@"\\.\" + drive.Name.TrimEnd('\\'), 0xC0000000, 3, IntPtr.Zero, 3, 0, IntPtr.Zero);
            if(deviceHandle == NativeMethods.INVALID_HANDLE_VALUE)
                throw new Win32Exception();

            this.deviceHandle = deviceHandle;

            var driveGeometry = new IOCTL.CDROM.DISK_GEOMETRY();
            unsafe
            {
                uint bytesReturned;
                if(!NativeMethods.DeviceIoControl(this.deviceHandle, IOCTL.CDROM.GET_DRIVE_GEOMETRY, IntPtr.Zero, 0, new IntPtr(&driveGeometry), (uint)sizeof(IOCTL.CDROM.DISK_GEOMETRY), out bytesReturned, IntPtr.Zero))
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
        public RawCDReader(string driveName)
            : this(new DriveInfo(driveName))
        {
        }
        #endregion

        #region Public Properties
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
            get { return this.diskSize; }
        }
        /// <summary>
        /// Gets or sets the position within the current stream.
        /// </summary>
        public override long Position
        {
            get { return (long)this.currentSector * this.sectorSize + this.currentSectorOffset; }
            set
            {
                uint sector = (uint)(value / this.sectorSize);
                uint offset = (uint)(value % this.sectorSize);
                long newPos;

                this.currentSectorOffset = offset;
                if(this.currentSector == sector)
                {
                    if(offset == 0)
                        NativeMethods.SetFilePointerEx(this.deviceHandle, sector * this.sectorSize, out newPos, 0);

                    return;
                }

                NativeMethods.SetFilePointerEx(this.deviceHandle, sector * this.sectorSize, out newPos, 0);
                this.currentSector = sector;
                if(offset != 0)
                    ReadCurrentSector();
            }
        }
        #endregion

        #region Public Methods
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

            int bytesRemaining = count;

            unsafe
            {
                if(this.currentSectorOffset > 0)
                {
                    int bufferCopyLength = Math.Min(bytesRemaining, (int)(this.sectorSize - this.currentSectorOffset));
                    Array.Copy(this.currentSectorBuffer, this.currentSectorOffset, buffer, offset, bufferCopyLength);
                    offset += bufferCopyLength;
                    bytesRemaining -= bufferCopyLength;
                    this.currentSectorOffset += (uint)bufferCopyLength;
                    if(this.currentSectorOffset >= this.sectorSize)
                    {
                        this.currentSector++;
                        this.currentSectorOffset = 0;
                    }
                }

                if(bytesRemaining == 0)
                    return count;

                if(bytesRemaining >= this.sectorSize)
                {
                    fixed(byte* ptr = &buffer[offset])
                    {
                        uint bytesReturned = 0;
                        if(!NativeMethods.ReadFile(this.deviceHandle, new IntPtr(ptr), (uint)bytesRemaining - ((uint)bytesRemaining % this.sectorSize), out bytesReturned, IntPtr.Zero))
                            throw new Win32Exception();

                        offset += (int)bytesReturned;
                        bytesRemaining -= (int)bytesReturned;
                        this.currentSector += bytesReturned / this.sectorSize;
                    }
                }

                if(bytesRemaining == 0)
                    return count;

                ReadCurrentSector();
                Array.Copy(this.currentSectorBuffer, this.currentSectorOffset, buffer, offset, bytesRemaining);
                this.currentSectorOffset += (uint)bytesRemaining;
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
            if(this.currentSectorOffset == 0)
                ReadCurrentSector();

            int value = this.currentSectorBuffer[this.currentSectorOffset++];
            if(this.currentSectorOffset >= this.sectorSize)
            {
                this.currentSectorOffset = 0;
                this.currentSector++;
            }

            return value;
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
        #endregion

        #region Protected Methods
        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.IO.Stream"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if(!this.disposed)
            {
                this.disposed = true;
                NativeMethods.CloseHandle(this.deviceHandle);
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Reads the current sector into the sector buffer.
        /// </summary>
        private void ReadCurrentSector()
        {
            unsafe
            {
                fixed(byte* ptr = &this.currentSectorBuffer[0])
                {
                    uint bytesReturned = 0;
                    if(!NativeMethods.ReadFile(this.deviceHandle, new IntPtr(ptr), this.sectorSize, out bytesReturned, IntPtr.Zero))
                        throw new Win32Exception();
                }
            }
        }
        #endregion
    }
}
