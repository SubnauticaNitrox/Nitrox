using System;
using System.Collections.Generic;
using System.IO;
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

    public async Task DownloadFileAsync(string url, string destinationPath, IProgress<(long bytesRead, long? totalBytes)>? progress = null, CancellationToken cancellationToken = default)
    {
        // Ensure we have an absolute URL
        string absoluteUrl = url;
        if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
        {
            absoluteUrl = $"https://nitrox.rux.gg/api/{url.TrimStart('/')}";
        }

        using HttpRequestMessage request = new(HttpMethod.Get, absoluteUrl);
        using HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        long? totalBytes = response.Content.Headers.ContentLength;
        await using Stream contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using FileStream fileStream = new(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

        byte[] buffer = new byte[8192];
        long totalBytesRead = 0;
        int bytesRead;

        while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
            totalBytesRead += bytesRead;
            progress?.Report((totalBytesRead, totalBytes));
        }
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
        public ArchitectureInfo? GetCurrentPlatformDownload()
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
