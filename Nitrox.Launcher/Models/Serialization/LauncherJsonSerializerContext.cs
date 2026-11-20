using System.Text.Json;
using System.Text.Json.Serialization;
using Nitrox.Launcher.Models.Services;
using Nitrox.Model.Serialization;

namespace Nitrox.Launcher.Models.Serialization;

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Default,
    PropertyNameCaseInsensitive = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    RespectNullableAnnotations = true,
    WriteIndented = false
    )]
[JsonSerializable(typeof(BackupMetadata))]
[JsonSerializable(typeof(SaveFileVersion))]
[JsonSerializable(typeof(NitroxBlogService.BlogPost[]))]
[JsonSerializable(typeof(NitroxWebsiteApiService.ChangeLog[]))]
[JsonSerializable(typeof(NitroxWebsiteApiService.NitroxRelease))]
internal sealed partial class LauncherJsonSerializerContext : JsonSerializerContext
{

}
