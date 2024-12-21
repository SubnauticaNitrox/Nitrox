using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using LitJson;
using Nitrox.Launcher.Models.Design;
using NitroxModel.Logger;

namespace Nitrox.Launcher.Models.Utils;

public partial class Downloader
{
    public const string BLOGS_URL = "https://nitroxblog.rux.gg/wp-json/wp/v2/posts?per_page=8&page=1";
    public const string LATEST_VERSION_URL = "https://nitrox.rux.gg/api/version/latest";
    public const string CHANGELOGS_URL = "https://nitrox.rux.gg/api/changelog/releases";
    public const string RELEASES_URL = "https://nitrox.rux.gg/api/version/releases";

    [GeneratedRegex(@"""version"":""([^""]*)""")]
    private static partial Regex JsonVersionFieldRegex { get; }

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

            JsonData data = JsonMapper.ToObject(jsonString);

            // TODO : Add a json schema validator
            for (int i = 0; i < data.Count; i++)
            {
                string released = (string)data[i]["date"];
                string url = (string)data[i]["link"];
                string title = WebUtility.HtmlDecode((string)data[i]["title"]["rendered"]);
                string imageUrl = (string)data[i]["jetpack_featured_media_url"];
                string imageCacheName = $"blogimage_{title.ReplaceInvalidFileNameCharacters().ToLowerInvariant()}";
                if (!DateTimeOffset.TryParse(released, out DateTimeOffset dateTime))
                {
                    dateTime = DateTimeOffset.UtcNow;
                    Log.Error($"Error while trying to parse release time ({released}) of blog {url}");
                }
                else
                {
                    imageCacheName = $"blogimage_{dateTime.ToUnixTimeSeconds()}";
                }
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
                                                                         HttpResponseMessage imageResponse = await GetResponseFromCacheAsync(imageUrl);
                                                                         return await imageResponse.Content.ReadAsByteArrayAsync();
                                                                     });
                using MemoryStream imageMemoryStream = new(imageData);
                Bitmap image = new(imageMemoryStream);

                blogs.Add(new NitroxBlog(title, DateOnly.FromDateTime(dateTime.DateTime), url, image));
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
            JsonData data = JsonMapper.ToObject(jsonString);

            // TODO : Add a json schema validator
            for (int i = 0; i < data.Count; i++)
            {
                string version = (string)data[i]["version"];
                string released = (string)data[i]["released"];
                JsonData patchnotes = data[i]["patchnotes"];

                if (!DateTime.TryParse(released, out DateTime dateTime))
                {
                    dateTime = DateTime.UtcNow;
                    Log.Error($"Error while trying to parse release time ({released}) of Nitrox v{version}");
                }

                builder.Clear();
                for (int j = 0; j < patchnotes.Count; j++)
                {
                    if (patchnotes[j].ToString().StartsWith('-'))
                    {
                        builder.AppendLine($"\n[b][u]{patchnotes[j].ToString().TrimStart('-', ' ')}[/u][/b]");
                    }
                    else
                    {
                        builder.AppendLine($"• {(string)patchnotes[j]}");
                    }
                }

                changelogs.Add(new NitroxChangelog(version, dateTime, builder.ToString()));
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

            Match match = JsonVersionFieldRegex.Match(jsonString);
            if (match.Success && match.Groups.Count > 1)
            {
                return new Version(match.Groups[1].Value);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"{nameof(Downloader)} : Error while fetching Nitrox version from {LATEST_VERSION_URL}");
            LauncherNotifier.Error("Unable to check for Nitrox updates");
            throw;
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
}
