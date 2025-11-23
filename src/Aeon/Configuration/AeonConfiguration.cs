using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Aeon.Emulator.Sound;

namespace Aeon.Emulator.Launcher.Configuration
{
    public sealed class AeonConfiguration
    {
        [JsonPropertyName("startup-path")]
        public string StartupPath { get; set; }
        [JsonPropertyName("launch")]
        public string Launch { get; set; }
        [JsonPropertyName("mouse-absolute")]
        public bool IsMouseAbsolute { get; set; }
        [JsonPropertyName("speed")]
        public int? EmulationSpeed { get; set; }
        [JsonPropertyName("hide-ui")]
        public bool HideUserInterface { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("physical-memory")]
        public int? PhysicalMemorySize { get; set; }
        [JsonPropertyName("midi-engine")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MidiEngine? MidiEngine { get; set; }

        [JsonPropertyName("drives")]
        public Dictionary<string, AeonDriveConfiguration> Drives { get; set; } = [];

        public static AeonConfiguration Load(Stream stream)
        {
            return JsonSerializer.Deserialize<AeonConfiguration>(stream);
        }
        public static AeonConfiguration Load(string fileName)
        {
            using var stream = File.OpenRead(fileName);
            return Load(stream);
        }
        public static AeonConfiguration GetQuickLaunchConfiguration(string hostPath, string launchTarget)
        {
            ArgumentNullException.ThrowIfNull(hostPath);

            var config = new AeonConfiguration
            {
                StartupPath = @"C:\",
                Launch = launchTarget,
                Drives =
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
}
