using Aeon.Emulator.Dos;
using Aeon.Emulator.Keyboard;

namespace Aeon.Emulator;

/// <summary>
/// Provides stream-based access to the emulated keyboard device.
/// </summary>
public sealed class ConsoleInStream : Stream, IDeviceStream
{
    private readonly KeyboardDevice keyboard;

    internal ConsoleInStream(KeyboardDevice keyboard)
    {
        ArgumentNullException.ThrowIfNull(keyboard);
        this.keyboard = keyboard;
    }

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => 0;
    public override long Position
    {
        get => 0;
        set => throw new NotSupportedException();
    }
    /// <summary>
    /// Gets a value indicating whether there is at least one byte of data available to be read.
    /// </summary>
    public bool DataAvailable => this.keyboard.HasTypeAheadDataAvailable;
    /// <summary>
    /// Gets information about the state of the device.
    /// </summary>
    public DosDeviceInfo DeviceInfo
    {
        get
        {
            var info = DosDeviceInfo.ConsoleInputDevice | DosDeviceInfo.SpecialDevice;
            if (this.DataAvailable)
                info |= DosDeviceInfo.NotEndOfFile;

            return info;
        }
    }

    public override void Flush()
    {
    }
    public override int Read(Span<byte> buffer)
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            ushort? value = this.keyboard.TryDequeueTypeAhead();
            if (value != null)
                buffer[i] = (byte)value;
            else
                return i;
        }

        return buffer.Length;
    }
    public override int Read(byte[] buffer, int offset, int count) => this.Read(buffer.AsSpan(offset, count));
    public override int ReadByte()
    {
        ushort? value = this.keyboard.TryDequeueTypeAhead();
        if (value != null)
            return (ushort)value & 0xFF;
        else
            return -1;
    }
    public override long Seek(long offset, SeekOrigin origin) => offset;
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count)
    {
    }
    public override void WriteByte(byte value)
    {
    }
    public override void Close()
    {
    }

    protected override void Dispose(bool disposing)
    {
        if (!disposing)
            base.Dispose(disposing);
    }
}
