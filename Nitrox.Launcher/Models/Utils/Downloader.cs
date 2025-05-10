using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Nitrox.Launcher.Models.Design;
using NitroxModel.Logger;

namespace Nitrox.Launcher.Models.Utils;

public static class Downloader
{
    public const string BLOGS_URL = "https://nitroxblog.rux.gg/wp-json/wp/v2/posts?per_page=8&page=1";
    public const string LATEST_VERSION_URL = "https://nitrox.rux.gg/api/version/latest";
    public const string CHANGELOGS_URL = "https://nitrox.rux.gg/api/changelog/releases";
    public const string RELEASES_URL = "https://nitrox.rux.gg/api/version/releases";

    private static readonly JsonSerializerOptions serializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public static async Task<IList<NitroxBlog>> GetBlogsAsync()
    {
        IList<NitroxBlog> blogs = new List<NitroxBlog>();

        try
        {
            string jsonString = await CacheFile.GetOrRefreshAsync("blogs",
                                                                  r => r.Read(""),
                                                                  (w, v) => w.Write(v),
                                                                  async () =>
                                                                  {
                                                                      using HttpResponseMessage response = await GetResponseFromCacheAsync(BLOGS_URL);
                                                                      return await response.Content.ReadAsStringAsync();
                                                                  });

            BlogPostJson[] data = JsonSerializer.Deserialize<BlogPostJson[]>(jsonString, serializerOptions);
            foreach (BlogPostJson blogPost in data)
            {
                string title = WebUtility.HtmlDecode(blogPost.Title.Rendered);
                string imageCacheName = $"blogimage_{blogPost.Released.ToUnixTimeSeconds()}";
                // Get image bitmap from image URL
                byte[] imageData = await CacheFile.GetOrRefreshAsync(imageCacheName,
                                                                     r => r.Read<byte[]>(),
                                                                     (w, v) =>
                                                                     {
                                                                         w.Write(v.Length);
                                                                         w.Write(v);
                                                                     },
                                                                     async () =>
                                                                     {
                                                                         HttpResponseMessage imageResponse = await GetResponseFromCacheAsync(blogPost.ThumbnailImageUrl);
                                                                         return await imageResponse.Content.ReadAsByteArrayAsync();
                                                                     },
                                                                     TimeSpan.FromDays(7));
                using MemoryStream imageMemoryStream = new(imageData);
                Bitmap image = new(imageMemoryStream);

                blogs.Add(new NitroxBlog(title, DateOnly.FromDateTime(blogPost.Released.DateTime), blogPost.PostUrl, image));
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"{nameof(Downloader)} : Error while fetching Nitrox blogs from {BLOGS_URL}");
            LauncherNotifier.Error("Unable to fetch Nitrox blogs");
        }

        return blogs;
    }

    public static async Task<IList<NitroxChangelog>> GetChangeLogsAsync()
    {
        IList<NitroxChangelog> changelogs = new List<NitroxChangelog>();

        try
        {
            //https://developer.wordpress.org/rest-api/reference/posts/#arguments
            string jsonString = await CacheFile.GetOrRefreshAsync("changelogs",
                                                                  r => r.Read(""),
                                                                  (w, v) => w.Write(v),
                                                                  async () =>
                                                                  {
                                                                      using HttpResponseMessage response = await GetResponseFromCacheAsync(CHANGELOGS_URL);
                                                                      return await response.Content.ReadAsStringAsync();
                                                                  });
            StringBuilder builder = new();
            foreach (ChangelogJson changelog in JsonSerializer.Deserialize<ChangelogJson[]>(jsonString, serializerOptions))
            {
                builder.Clear();
                foreach (string patchNote in changelog.PatchNotes)
                {
                    if (string.IsNullOrWhiteSpace(patchNote))
                    {
                        continue;
                    }

                    if (patchNote.StartsWith('-'))
                    {
                        builder.AppendLine($"\n[b][u]{patchNote.TrimStart('-', ' ')}[/u][/b]");
                    }
                    else
                    {
                        builder.AppendLine($"• {patchNote}");
                    }
                }

                changelogs.Add(new NitroxChangelog(changelog.Version, changelog.Released.DateTime, builder.ToString()));
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"{nameof(Downloader)} : Error while fetching Nitrox changelogs from {CHANGELOGS_URL}");
            LauncherNotifier.Error("Unable to fetch Nitrox changelogs");
        }

        return changelogs;
    }

    public static async Task<Version> GetNitroxLatestVersionAsync()
    {
        try
        {
            string jsonString = await CacheFile.GetOrRefreshAsync("update",
                                                                  r => r.Read(""),
                                                                  (w, v) => w.Write(v),
                                                                  async () =>
                                                                  {
                                                                      using HttpResponseMessage response = await GetResponseFromCacheAsync(LATEST_VERSION_URL);
                                                                      return await response.Content.ReadAsStringAsync();
                                                                  });

            UpdateJson updateJson = JsonSerializer.Deserialize<UpdateJson>(jsonString, serializerOptions);
            return updateJson.Version;
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"{nameof(Downloader)} : Error while fetching Nitrox version from {LATEST_VERSION_URL}");
            LauncherNotifier.Error("Unable to check for Nitrox updates");
        }

        return new Version();
    }

    private static async Task<HttpResponseMessage> GetResponseFromCacheAsync(string url)
    {
        Log.Info($"Trying to request data from {url}");

        using HttpClient client = new();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Nitrox.Launcher");
        client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { MaxAge = TimeSpan.FromDays(1) };
        client.Timeout = TimeSpan.FromSeconds(5);

        try
        {
            return await client.GetAsync(url);
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Error while requesting data from {url}");
        }

        return null;
    }

    private record BlogPostJson
    {
        /// <summary>
        ///     Release time of this blog post.
        /// </summary>
        [JsonPropertyName("date")]
        public DateTimeOffset Released { get; init; }

        [JsonPropertyName("link")]
        public string PostUrl { get; init; }

        [JsonPropertyName("title")]
        public BlogPostTitle Title { get; init; }

        [JsonPropertyName("jetpack_featured_media_url")]
        public string ThumbnailImageUrl { get; init; }

        public record BlogPostTitle
        {
            [JsonPropertyName("rendered")]
            public string Rendered { get; init; }
        }
    }

    private record ChangelogJson
    {
        /// <summary>
        ///     Release time of this change log.
        /// </summary>
        [JsonPropertyName("released")]
        public DateTimeOffset Released { get; init; }

        [JsonPropertyName("patchnotes")]
        public string[] PatchNotes { get; init; }

        [JsonPropertyName("version")]
        public string Version { get; init; }
    }

    private record UpdateJson
    {
        /// <summary>
        ///     Url pointing to the zip file to download this Nitrox release.
        /// </summary>
        [JsonPropertyName("url")]
        public string DownloadUrl { get; init; }

        [JsonPropertyName("filesize")]
        public float FileSizeMegaBytes { get; init; }

        [JsonPropertyName("version")]
        public Version Version { get; init; }

        /// <summary>
        ///     Hash to verify that the download was received as expected.
        /// </summary>
        [JsonPropertyName("md5")]
        public string Md5Hash { get; init; }
    }
}
