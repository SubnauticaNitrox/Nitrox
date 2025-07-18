using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Nitrox.Launcher.Models.Attributes;
using Nitrox.Launcher.Models.Design;

namespace Nitrox.Launcher.Models.Services;

[HttpService]
internal sealed class NitroxWebsiteApiService
{
    private readonly HttpClient httpClient;

    public NitroxWebsiteApiService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
        httpClient.BaseAddress = new Uri("https://nitrox.rux.gg/api/");
    }

    public async Task<NitroxChangelog[]?> GetChangeLogsAsync(CancellationToken cancellationToken = default)
    {
        ChangeLog[] changeLogs = await httpClient.GetFromJsonAsync<ChangeLog[]>("changelog/releases", cancellationToken: cancellationToken);
        NitroxChangelog[] result = new NitroxChangelog[changeLogs.Length];
        StringBuilder buffer = new();
        for (int i = 0; i < changeLogs.Length; i++)
        {
            result[i] = changeLogs[i].FromDtoToLauncher(buffer);
        }
        return result;
    }

    public async Task<NitroxRelease?> GetNitroxLatestVersionAsync() => await httpClient.GetFromJsonAsync<NitroxRelease>("version/latest");

    public sealed record ChangeLog
    {
        /// <summary>
        ///     Release time of this change log.
        /// </summary>
        [JsonPropertyName("released")]
        public required DateTimeOffset Released { get; init; }

        [JsonPropertyName("patchnotes")]
        public required string[] PatchNotes { get; init; }

        [JsonPropertyName("version")]
        public required string Version { get; init; }
    }

    public sealed record NitroxRelease
    {
        /// <summary>
        ///     Url pointing to the zip file to download this Nitrox release.
        /// </summary>
        [JsonPropertyName("url")]
        public required string DownloadUrl { get; init; }

        [JsonPropertyName("filesize")]
        public required float FileSizeMegaBytes { get; init; }

        [JsonPropertyName("version")]
        public required Version Version { get; init; }

        /// <summary>
        ///     Hash to verify that the download was received as expected.
        /// </summary>
        [JsonPropertyName("md5")]
        public required string Md5Hash { get; init; }
    }
}
