using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nitrox.Launcher.Models.HttpDelegatingHandlers;

/// <summary>
///     Reuses GET request tasks to prevent redundant requests.
/// </summary>
internal sealed class CacheGetRequestTaskDelegatingHandler : DelegatingHandler
{
    private static readonly Dictionary<string, Task<byte[]>> cache = [];
    private static readonly Lock cacheLocker = new();

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request is not { Method: { } method, RequestUri: not null } || method != HttpMethod.Get)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        // Cache request in background, ignoring cancellation.
        string url = request.RequestUri.ToString();
        Task<byte[]> task;
        lock (cacheLocker)
        {
            if (!cache.TryGetValue(url, out task) || task is { IsCompleted: true, IsCompletedSuccessfully: false })
            {
                task = UncancellableRequest(request);
                cache.TryAdd(url, task);
            }
        }

        return CreateResponseFromRequestData(await task.WaitAsync(cancellationToken));
    }

    private async Task<byte[]> UncancellableRequest(HttpRequestMessage request)
    {
        using HttpResponseMessage response = await base.SendAsync(request, CancellationToken.None);
        byte[] data = await response.Content.ReadAsByteArrayAsync(CancellationToken.None);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(Encoding.UTF8.GetString(data));
        }
        return data;
    }

    private static HttpResponseMessage CreateResponseFromRequestData(byte[] data) => new(HttpStatusCode.OK) { Content = new ReadOnlyMemoryContent(data) };
}
