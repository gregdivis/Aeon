using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Aeon.DiskImages.Archives;
using YamlDotNet.Serialization;

namespace Aeon.Emulator.Launcher.Configuration
{
    public sealed class AeonConfiguration
    {
        [YamlMember(Alias = "startup-path")]
        public string StartupPath { get; set; }
        [YamlMember(Alias = "launch")]
        public string Launch { get; set; }
        [YamlMember(Alias = "mouse-absolute")]
        public bool IsMouseAbsolute { get; set; }
        [YamlMember(Alias = "speed")]
        public int? EmulationSpeed { get; set; }
        [YamlMember(Alias = "hide-ui")]
        public bool HideUserInterface { get; set; }
        [YamlMember(Alias = "title")]
        public string Title { get; set; }
        [YamlMember(Alias = "id")]
        public string Id { get; set; }
        [YamlMember(Alias = "physical-memory")]
        public int? PhysicalMemorySize { get; set; }

        [YamlMember(Alias = "drives")]
        public Dictionary<string, AeonDriveConfiguration> Drives { get; set; } = new Dictionary<string, AeonDriveConfiguration>();

        [YamlIgnore]
        public ArchiveFile Archive { get; private set; }

        public static AeonConfiguration Load(TextReader reader)
        {
            var deserializer = new Deserializer();
            return deserializer.Deserialize<AeonConfiguration>(reader);
        }
        public static AeonConfiguration Load(Stream stream)
        {
            using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            return Load(reader);
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
