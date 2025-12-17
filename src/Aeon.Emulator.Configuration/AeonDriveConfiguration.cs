namespace Aeon.Emulator.Configuration;

public sealed class AeonDriveConfiguration
{
    public DriveType Type { get; set; }
    public string? HostPath { get; set; }
    public bool ReadOnly { get; set; }
    public string? ImagePath { get; set; }
    public long? FreeSpace { get; set; }
    public string? Label { get; set; }
}
