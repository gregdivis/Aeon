using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Aeon.DiskImages.Archives;

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

        [JsonPropertyName("drives")]
        public Dictionary<string, AeonDriveConfiguration> Drives { get; set; } = new Dictionary<string, AeonDriveConfiguration>();

        [JsonIgnore]
        public ArchiveFile Archive { get; private set; }

        public static AeonConfiguration Load(Stream stream)
        {
            return JsonSerializer.Deserialize<AeonConfiguration>(stream);
        }
        public static AeonConfiguration Load(string fileName)
        {
            if (fileName.EndsWith(".AeonPack", StringComparison.OrdinalIgnoreCase))
                return LoadArchive(new ArchiveFile(File.OpenRead(fileName)));

            using var stream = File.OpenRead(fileName);
            return Load(stream);
        }
        public static AeonConfiguration GetQuickLaunchConfiguration(string hostPath, string launchTarget)
        {
            if (hostPath == null)
                throw new ArgumentNullException(nameof(hostPath));

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

        private static AeonConfiguration LoadArchive(ArchiveFile archive)
        {
            using var configStream = archive.OpenItem("Archive.AeonConfig");
            if (configStream == null)
                throw new InvalidDataException("Missing configuration in archive.");

            var config = Load(configStream);
            config.Archive = archive;
            return config;
        }
    }
}
