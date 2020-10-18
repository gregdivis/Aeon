using System;
using System.IO;
using YamlDotNet.Serialization;

namespace Aeon.Emulator.Launcher.Configuration
{
    public sealed class GlobalConfiguration
    {
        [YamlMember(Alias = "mt32-roms-path")]
        public string Mt32RomsPath { get; set; }
        [YamlMember(Alias = "mt32-enabled")]
        public bool Mt32Enabled { get; set; }

        public static GlobalConfiguration Load()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aeon Emulator", "AeonConfig.yaml");
            if (File.Exists(path))
            {
                using var reader = File.OpenText(path);
                var deserializer = new Deserializer();
                return deserializer.Deserialize<GlobalConfiguration>(reader);
            }
            else
            {
                return new GlobalConfiguration();
            }
        }
    }
}
