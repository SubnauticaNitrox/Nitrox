using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nitrox.Model.Helper;
using Serilog;

namespace Nitrox.Launcher.Models.HttpDelegatingHandlers;

internal sealed class FileCacheDelegatingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Method != HttpMethod.Get)
        {
            return await base.SendAsync(request, cancellationToken);
        }
        if (request.Headers.CacheControl is not { MaxAge: { } cacheMaxAge })
        {
            return await base.SendAsync(request, cancellationToken);
        }
        if (GetCacheFilePathForRequest(request) is not { } cacheFilePath)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        // Try load data from cache file.
        try
        {
            DateTime lastWrite = File.GetLastWriteTimeUtc(cacheFilePath);
            if (DateTimeOffset.UtcNow - lastWrite < cacheMaxAge)
            {
                byte[] cacheData = await File.ReadAllBytesAsync(cacheFilePath, cancellationToken);
                if (cacheData is { Length: > 0 })
                {
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ReadOnlyMemoryContent(cacheData) };
                }
            }
        }
        catch (IOException)
        {
            // ignored
        }
        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
        if (response is { StatusCode: HttpStatusCode.OK, Content: { } content })
        {
            await File.WriteAllBytesAsync(cacheFilePath, await content.ReadAsByteArrayAsync(cancellationToken), cancellationToken);
        }
        return response;
    }

    private static string? GetCacheFilePathForRequest(HttpRequestMessage request)
    {
        if (request.RequestUri is not { } uri)
        {
            throw new Exception($"{nameof(request.RequestUri)} must not be null");
        }
        try
        {
            Directory.CreateDirectory(NitroxUser.CachePath);
            return Path.Combine(NitroxUser.CachePath, $"nitrox_{string.Join('_', $"{uri.Host}{uri.LocalPath}".ReplaceInvalidFileNameCharacters('_').Split('_').Select(s => s[0]))}_{Convert.ToHexStringLower(uri.ToString().AsMd5Hash())}.cache");
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Failed to generate cache file for request: {uri}");
            return null;
        }
    }
}
