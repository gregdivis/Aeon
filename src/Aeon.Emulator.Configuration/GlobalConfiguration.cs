using System.Text.Json;
using Aeon.Emulator.Sound;

namespace Aeon.Emulator.Configuration;

public sealed class GlobalConfiguration
{
    public string? Mt32RomsPath { get; set; }
    public bool Mt32Enabled { get; set; }
    public string? SoundfontPath { get; set; }
    public MidiEngine? MidiEngine { get; set; }

    public static GlobalConfiguration Load()
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aeon Emulator", "AeonConfig.json");
        if (File.Exists(path))
        {
            using var stream = File.OpenRead(path);
            return JsonSerializer.Deserialize(stream, AeonConfigJsonSerializerContext.Default.GlobalConfiguration) ?? new GlobalConfiguration();
        }
        else
        {
            return new GlobalConfiguration();
        }
    }
}
