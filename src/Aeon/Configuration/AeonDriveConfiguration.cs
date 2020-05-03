using YamlDotNet.Serialization;

namespace Aeon.Emulator.Launcher.Configuration
{
    public sealed class AeonDriveConfiguration
    {
        [YamlMember(Alias = "type")]
        public DriveType Type { get; set; }
        [YamlMember(Alias = "host-path")]
        public string HostPath { get; set; }
        [YamlMember(Alias = "read-only")]
        public bool ReadOnly { get; set; }
        [YamlMember(Alias = "image-path")]
        public string ImagePath { get; set; }
        [YamlMember(Alias = "free-space")]
        public long? FreeSpace { get; set; }
        [YamlMember(Alias = "label")]
        public string Label { get; set; }
    }
}
