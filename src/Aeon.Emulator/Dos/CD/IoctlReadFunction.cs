namespace Aeon.Emulator.Dos.CD;

internal static class IoctlReadFunction
{
    public const byte DeviceHeaderAddress = 1;
    public const byte DeviceStatus = 6;
    public const byte VolumeSize = 8;
    public const byte AudioDiscInfo = 10;
    public const byte AudioTrackInfo = 11;
    public const byte AudioQChannelInfo = 12;
    public const byte AudioStatusInfo = 15;
}
