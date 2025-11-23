namespace Aeon.Emulator.Dos;

internal sealed class NullStream : Stream, IDeviceStream
{
    public static readonly NullStream Instance = new();

    private NullStream()
    {
    }

    public DosDeviceInfo DeviceInfo => DosDeviceInfo.NullDevice | DosDeviceInfo.SpecialDevice;
    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length => 0;
    public override long Position
    {
        get => 0;
        set { }
    }

    public override void Flush()
    {
    }
    public override int Read(byte[] buffer, int offset, int count) => 0;
    public override long Seek(long offset, SeekOrigin origin) => 0;
    public override void SetLength(long value)
    {
    }
    public override void Write(byte[] buffer, int offset, int count)
    {
    }
}
