using System.Text.Json.Serialization;

namespace Aeon.Emulator.Launcher.Configuration;

[JsonSerializable(typeof(AeonConfiguration))]
[JsonSerializable(typeof(GlobalConfiguration))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.KebabCaseLower, UseStringEnumConverter = true)]
internal sealed partial class AeonConfigJsonSerializerContext : JsonSerializerContext
{
}
