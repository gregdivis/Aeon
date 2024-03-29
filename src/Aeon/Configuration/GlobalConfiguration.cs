﻿using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Aeon.Emulator.Sound;

namespace Aeon.Emulator.Launcher.Configuration
{
    public sealed class GlobalConfiguration
    {
        [JsonPropertyName("mt32-roms-path")]
        public string Mt32RomsPath { get; set; }
        [JsonPropertyName("mt32-enabled")]
        public bool Mt32Enabled { get; set; }
        [JsonPropertyName("soundfont-path")]
        public string SoundFontPath { get; set; }
        [JsonPropertyName("midi-engine")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MidiEngine? MidiEngine { get; set; }

        public static GlobalConfiguration Load()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aeon Emulator", "AeonConfig.json");
            if (File.Exists(path))
            {
                using var stream = File.OpenRead(path);
                return JsonSerializer.Deserialize<GlobalConfiguration>(stream);
            }
            else
            {
                return new GlobalConfiguration();
            }
        }
    }
}
