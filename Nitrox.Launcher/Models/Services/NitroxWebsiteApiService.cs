using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Nitrox.Launcher.Models.Attributes;
using Nitrox.Launcher.Models.Design;
using Nitrox.Model.Core;

namespace Nitrox.Launcher.Models.Services;

[HttpService]
internal sealed class NitroxWebsiteApiService
{
    private readonly HttpClient httpClient;
    private readonly HttpFileService httpFileService;

    public NitroxWebsiteApiService(HttpClient httpClient, HttpFileService httpFileService)
    {
        this.httpClient = httpClient;
        this.httpFileService = httpFileService;
        httpClient.BaseAddress = new Uri("https://nitrox.rux.gg/api/");
    }

    public async Task<NitroxChangelog[]?> GetChangeLogsAsync(CancellationToken cancellationToken = default)
    {
        ChangeLog[] changeLogs = await httpClient.GetFromJsonAsync<ChangeLog[]>("changelog/releases", cancellationToken);
        NitroxChangelog[] result = new NitroxChangelog[changeLogs.Length];
        StringBuilder buffer = new();
        for (int i = 0; i < changeLogs.Length; i++)
        {
            result[i] = changeLogs[i].FromDtoToLauncher(buffer);
        }
        return result;
    }

    public async Task<NitroxRelease?> GetNitroxLatestVersionAsync() => await httpClient.GetFromJsonAsync<NitroxRelease>("version/latest");

    /// <summary>
    ///     Gets the latest Nitrox for the platform of the current machine.
    /// </summary>
    public async Task<HttpFileService.FileDownloader?> GetLatestNitroxAsync(CancellationToken cancellationToken)
    {
        if (await GetNitroxLatestVersionAsync() is not { CurrentPlatformInfo: { } downloadInfo })
        {
            return null;
        }

        return await httpFileService.GetFileAsync(downloadInfo.DownloadUrl, cancellationToken);
    }

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
        [JsonPropertyName("version")]
        public required Version Version { get; init; }

        [JsonPropertyName("platforms")]
        public Dictionary<string, PlatformInfo>? Platforms { get; init; }

        /// <summary>
        ///     Gets the download info for the current platform and architecture.
        /// </summary>
        public ArchitectureInfo? CurrentPlatformInfo
        {
            get
            {
                if (Platforms == null)
                {
                    return null;
                }

                if (Platforms.TryGetValue(NitroxEnvironment.PlatformName, out PlatformInfo? platformInfo) &&
                    platformInfo?.Architectures != null &&
                    platformInfo.Architectures.TryGetValue(NitroxEnvironment.ArchitectureName, out ArchitectureInfo? archInfo))
                {
                    return archInfo;
                }

                return null;
            }
        }
    }

    public sealed record PlatformInfo
    {
        [JsonPropertyName("filesize")]
        public string? FileSize { get; init; }

        [JsonPropertyName("architectures")]
        public Dictionary<string, ArchitectureInfo>? Architectures { get; init; }
    }

    public sealed record ArchitectureInfo
    {
        [JsonPropertyName("url")]
        public required string DownloadUrl { get; init; }

        [JsonPropertyName("md5")]
        public required string Md5Hash { get; init; }

        [JsonPropertyName("filesize")]
        public required string FileSize { get; init; }

        public float FileSizeMegaBytes => float.TryParse(FileSize, out float size) ? size : 0;
    }
}
