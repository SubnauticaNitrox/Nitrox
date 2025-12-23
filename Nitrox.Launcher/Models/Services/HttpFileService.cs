using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nitrox.Launcher.Models.Attributes;

namespace Nitrox.Launcher.Models.Services;

[HttpService]
internal sealed class HttpFileService
{
    private readonly HttpClient httpClient;

    public HttpFileService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
        this.httpClient.DefaultRequestHeaders.CacheControl = null;
    }

    public async Task<FileDownloader> GetFileStreamAsync(string url, CancellationToken cancellationToken = default)
    {
        // Ensure we have an absolute URL
        if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
        {
            throw new Exception($"URL must be absolute path: '{url}'");
        }

        using HttpRequestMessage request = new(HttpMethod.Get, url);
        HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        try
        {
            response.EnsureSuccessStatusCode();
        }
        catch
        {
            response.Dispose();
            throw;
        }

        return new FileDownloader(await response.Content.ReadAsStreamAsync(cancellationToken), response.Content.Headers.ContentLength ?? 0, response, cancellationToken);
    }

    public record FileDownloader(Stream Stream, long SizeFromServer, IDisposable? Disposable = null, CancellationToken CancellationToken = default)
        : IDisposable
    {
        private FileStream? destinationFileStream;

        public async IAsyncEnumerable<long> DownloadToFileInStepsAsync(string destinationFilePath)
        {
            destinationFileStream ??= new(destinationFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 8192, true);

            byte[] buffer = new byte[8192];
            long totalBytesRead = 0;
            int bytesRead;

            while ((bytesRead = await Stream.ReadAsync(buffer, CancellationToken)) > 0)
            {
                await destinationFileStream.WriteAsync(buffer.AsMemory(0, bytesRead), CancellationToken);
                totalBytesRead += bytesRead;
                yield return totalBytesRead;
            }
        }

        public void Dispose()
        {
            Stream.Dispose();
            destinationFileStream?.Dispose();
            Disposable?.Dispose();
        }
    }
}
