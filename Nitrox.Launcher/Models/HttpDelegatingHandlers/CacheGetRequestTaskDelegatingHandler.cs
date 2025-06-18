using System;
using System.Collections.Concurrent;
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
    private static readonly ConcurrentDictionary<string, Task<byte[]>> cache = [];

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Method != HttpMethod.Get)
        {
            return await base.SendAsync(request, cancellationToken);
        }
        string? url = request.RequestUri == null ? null : $"{request.RequestUri.Scheme}://{request.RequestUri.Host}{request.RequestUri.LocalPath}";
        if (url == null)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        // Cache request in background, ignoring cancellation.
        if (!cache.TryGetValue(url, out Task<byte[]> task) || task is { IsCompleted: true, IsCompletedSuccessfully: false })
        {
            task = Task.Run(async () =>
            {
                using HttpResponseMessage response = await base.SendAsync(request, CancellationToken.None);
                byte[] data = await response.Content.ReadAsByteArrayAsync(CancellationToken.None);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(Encoding.UTF8.GetString(data));
                }
                return data;
            }, CancellationToken.None);
            cache.TryAdd(url, task);
        }
        return CreateResponseFromData(await task.WaitAsync(cancellationToken));
    }

    private static HttpResponseMessage CreateResponseFromData(byte[] data) => new(HttpStatusCode.OK) { Content = new ReadOnlyMemoryContent(data) };
}
