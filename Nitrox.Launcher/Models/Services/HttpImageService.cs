using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Nitrox.Launcher.Models.Attributes;

namespace Nitrox.Launcher.Models.Services;

[HttpService]
internal sealed class HttpImageService
{
    private readonly HttpClient httpClient;

    public HttpImageService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
        httpClient.DefaultRequestHeaders.CacheControl!.MaxAge = TimeSpan.FromDays(7);
    }

    public async Task<Bitmap> GetImageAsync(string url, CancellationToken cancellationToken = default)
    {
        byte[] imageBytes = await httpClient.GetByteArrayAsync(url, cancellationToken);
        using MemoryStream imageMemoryStream = new(imageBytes);
        return new(imageMemoryStream);
    }
}
