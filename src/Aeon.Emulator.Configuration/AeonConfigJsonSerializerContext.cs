using System.Text.Json.Serialization;

namespace Aeon.Emulator.Configuration;

[JsonSerializable(typeof(AeonConfiguration))]
[JsonSerializable(typeof(GlobalConfiguration))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.KebabCaseLower, UseStringEnumConverter = true, GenerationMode = JsonSourceGenerationMode.Metadata)]
internal sealed partial class AeonConfigJsonSerializerContext : JsonSerializerContext
{
}
