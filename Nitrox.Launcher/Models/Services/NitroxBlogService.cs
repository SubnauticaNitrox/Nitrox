using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading;
using Nitrox.Launcher.Models.Attributes;
using Nitrox.Launcher.Models.Design;

namespace Nitrox.Launcher.Models.Services;

[HttpService]
internal sealed class NitroxBlogService
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

    public async IAsyncEnumerable<NitroxBlog> GetBlogPostsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        BlogPost[] blogs = await httpClient.GetFromJsonAsync<BlogPost[]>("posts?per_page=8&page=1", cancellationToken);
        foreach (BlogPost blog in blogs)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return blog.FromDtoToLauncher(await httpImageService.GetImageAsync(blog.ThumbnailImageUrl, cancellationToken));
        }
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
