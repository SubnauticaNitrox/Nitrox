using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Nitrox.Launcher.Models.Design;

namespace Nitrox.Launcher.Models.Services;

internal class NitroxBlogService
{
    private readonly HttpClient httpClient;
    private readonly HttpImageService httpImageService;

    public NitroxBlogService(HttpClient httpClient, HttpImageService httpImageService)
    {
        this.httpClient = httpClient;
        this.httpImageService = httpImageService;
        httpClient.BaseAddress = new Uri("https://nitroxblog.rux.gg/wp-json/wp/v2/");
        httpClient.DefaultRequestHeaders.CacheControl!.MaxAge = TimeSpan.FromDays(7);
    }

    public async Task<NitroxBlog[]?> GetBlogPostsAsync(CancellationToken cancellationToken = default)
    {
        BlogPost[] blogs = await httpClient.GetFromJsonAsync<BlogPost[]>("posts?per_page=8&page=1", cancellationToken: cancellationToken);
        NitroxBlog[] result = new NitroxBlog[blogs.Length];
        for (int i = 0; i < blogs.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            BlogPost item = blogs[i];
            result[i] = item.FromDtoToLauncher(await httpImageService.GetImageAsync(item.ThumbnailImageUrl, cancellationToken));
        }
        return result;
    }

    public sealed record BlogPost
    {
        /// <summary>
        ///     Release time of this blog post.
        /// </summary>
        [JsonPropertyName("date")]
        public required DateTimeOffset Released { get; init; }

        [JsonPropertyName("link")]
        public required string PostUrl { get; init; }

        [JsonPropertyName("title")]
        public required BlogPostTitle Title { get; init; }

        [JsonPropertyName("jetpack_featured_media_url")]
        public required string ThumbnailImageUrl { get; init; }

        public sealed record BlogPostTitle
        {
            [JsonPropertyName("rendered")]
            public required string Rendered { get; init; }
        }
    }
}
