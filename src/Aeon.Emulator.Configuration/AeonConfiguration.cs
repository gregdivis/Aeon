using System.Text.Json;
using System.Text.Json.Serialization;
using Aeon.Emulator.Sound;

namespace Aeon.Emulator.Configuration;

public sealed class AeonConfiguration
{
    public string? StartupPath { get; set; }
    public string? Launch { get; set; }
    [JsonPropertyName("mouse-absolute")]
    public bool? IsMouseAbsolute { get; set; }
    [JsonPropertyName("speed")]
    public int? EmulationSpeed { get; set; }
    public string? Title { get; set; }
    public string? Id { get; set; }
    [JsonPropertyName("physical-memory")]
    public int? PhysicalMemorySize { get; set; }
    public MidiEngine? MidiEngine { get; set; }

    public Dictionary<string, AeonDriveConfiguration>? Drives { get; set; }

    public static AeonConfiguration Load(string fileName)
    {
        ArgumentException.ThrowIfNullOrEmpty(fileName);

        using var stream = File.OpenRead(fileName);
        return JsonSerializer.Deserialize(stream, AeonConfigJsonSerializerContext.Default.AeonConfiguration) ?? new AeonConfiguration();
    }

    public static AeonConfiguration GetQuickLaunchConfiguration(string hostPath, string launchTarget)
    {
        ArgumentNullException.ThrowIfNull(hostPath);

        var config = new AeonConfiguration
        {
            StartupPath = @"C:\",
            Launch = launchTarget,
            Drives = new Dictionary<string, AeonDriveConfiguration>
            {
                ["C"] = new AeonDriveConfiguration
                {
                    Type = DriveType.Fixed,
                    HostPath = hostPath
                }
            }
        };

        return config;
    }
}
