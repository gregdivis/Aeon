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
    public string? Mt32RomsPath { get; set; }
    public bool? Mt32Enabled { get; set; }
    public string? SoundfontPath { get; set; }
    public bool? ShowIps { get; set; }

    public Dictionary<string, AeonDriveConfiguration>? Drives { get; set; }

    public static AeonConfiguration Load(string fileName)
    {
        ArgumentException.ThrowIfNullOrEmpty(fileName);

        using var stream = File.OpenRead(fileName);
        var config = JsonSerializer.Deserialize(stream, AeonConfigJsonSerializerContext.Default.AeonConfiguration) ?? new AeonConfiguration();
        config.MergeFrom(LoadGlobalConfig());
        return config;
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

        config.MergeFrom(LoadGlobalConfig());
        return config;
    }

    public static AeonConfiguration? LoadGlobalConfig()
    {
        using var stream = TryOpen(Path.Combine(Environment.CurrentDirectory, ".AeonConfig"))
            ?? TryOpen(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".AeonConfig"))
            ?? TryOpenWindows(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aeon Emulator", "AeonConfig.json"));

        AeonConfiguration? config = null;
        if (stream is not null)
            config = JsonSerializer.Deserialize(stream, AeonConfigJsonSerializerContext.Default.AeonConfiguration);

        return config;
    }

    private static FileStream? TryOpenWindows(string? path) => OperatingSystem.IsWindows() ? TryOpen(path) : null;
    private static FileStream? TryOpen(string? path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
            return null;

        try
        {
            return File.OpenRead(path);
        }
        catch
        {
            return null;
        }
    }


    public void MergeFrom(AeonConfiguration? other)
    {
        if (other is null)
            return;

        this.StartupPath ??= other.StartupPath;
        this.Launch ??= other.Launch;
        this.IsMouseAbsolute ??= other.IsMouseAbsolute;
        this.EmulationSpeed ??= other.EmulationSpeed;
        this.Title ??= other.Title;
        this.Id ??= other.Id;
        this.PhysicalMemorySize ??= other.PhysicalMemorySize;
        this.MidiEngine ??= other.MidiEngine;
        this.Mt32RomsPath ??= other.Mt32RomsPath;
        this.Mt32Enabled ??= other.Mt32Enabled;
        this.SoundfontPath ??= other.SoundfontPath;
        this.ShowIps ??= other.ShowIps;

        this.Drives ??= other.Drives;
    }
}
