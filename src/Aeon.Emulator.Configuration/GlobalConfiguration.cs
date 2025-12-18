using System.Text.Json;
using Aeon.Emulator.Sound;

namespace Aeon.Emulator.Configuration;

public sealed class GlobalConfiguration
{
    public string? Mt32RomsPath { get; set; }
    public bool Mt32Enabled { get; set; }
    public string? SoundfontPath { get; set; }
    public MidiEngine? MidiEngine { get; set; }
    public bool ShowIps { get; set; }

    public static GlobalConfiguration Load()
    {
        using var stream = TryOpen(Path.Combine(Environment.CurrentDirectory, ".AeonConfig"))
            ?? TryOpen(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".AeonConfig"))
            ?? TryOpenWindows(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aeon Emulator", "AeonConfig.json"));

        GlobalConfiguration? config = null;
        if (stream is not null)
            config = JsonSerializer.Deserialize(stream, AeonConfigJsonSerializerContext.Default.GlobalConfiguration);

        return config ?? new GlobalConfiguration();
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
}
