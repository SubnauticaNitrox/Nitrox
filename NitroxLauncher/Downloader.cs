using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LitJson;
using NitroxLauncher.Models;

namespace NitroxLauncher
{
    internal static class Downloader
    {
        public const string BLOGS_URL = "https://nitroxblog.rux.gg/wp-json/wp/v2/posts?per_page=8&page=1";
        public const string LATEST_VERSION_URL = "https://nitrox.rux.gg/api/version/latest";
        public const string CHANGELOGS_URL = "https://nitrox.rux.gg/api/changelog/releases";
        public const string RELEASES_URL = "https://nitrox.rux.gg/api/version/releases";

        // Create a policy that allows any cache to supply requested resources if the resource on the server is not newer than the cached copy
        private static readonly HttpRequestCachePolicy cachePolicy = new(
            HttpCacheAgeControl.MaxAge,
            TimeSpan.FromDays(1)
        );

        public static async Task<IList<NitroxBlog>> GetBlogs()
        {
            IList<NitroxBlog> blogs = new List<NitroxBlog>();

            try
            {
                using WebResponse response = await GetResponseFromCache(BLOGS_URL);

                if (response.IsFromCache)
                {
                    Log.Info("Fetched nitrox blogs from the local cache");
                }

                using (StreamReader sr = new(response.GetResponseStream()))
                {
                    string json = sr.ReadToEnd();
                    JsonData data = JsonMapper.ToObject(json);

                    // TODO : Add a json schema validator 
                    for (int i = 0; i < data.Count; i++)
                    {
                        string released = (string)data[i]["date"];
                        string url = (string)data[i]["link"];
                        string title = (string)data[i]["title"]["rendered"];
                        string image = (string)data[i]["jetpack_featured_media_url"];

                        if (!DateTime.TryParse(released, out DateTime dateTime))
                        {
                            dateTime = DateTime.UtcNow;
                            Log.Error($"Error while trying to parse release time ({released}) of blog {url}");
                        }

                        blogs.Add(new NitroxBlog(title, dateTime, url, image));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(Downloader)} : Error while fetching nitrox blogs from {BLOGS_URL}");
                LauncherNotifier.Error("Unable to fetch nitrox blogs");
            }

            return blogs;
        }

        public async static Task<IList<NitroxChangelog>> GetChangeLogs()
        {
            IList<NitroxChangelog> changelogs = new List<NitroxChangelog>();

            try
            {
                //https://developer.wordpress.org/rest-api/reference/posts/#arguments
                using WebResponse response = await GetResponseFromCache(CHANGELOGS_URL);

                if (response.IsFromCache)
                {
                    Log.Info("Fetched nitrox changelogs from the local cache");
                }

                using (StreamReader sr = new(response.GetResponseStream()))
                {
                    string json = sr.ReadToEnd();
                    StringBuilder builder = new();
                    JsonData data = JsonMapper.ToObject(json);

                    // TODO : Add a json schema validator 
                    for (int i = 0; i < data.Count; i++)
                    {
                        string version = (string)data[i]["version"];
                        string released = (string)data[i]["released"];
                        JsonData patchnotes = data[i]["patchnotes"];

                        if (!DateTime.TryParse(released, out DateTime dateTime))
                        {
                            dateTime = DateTime.UtcNow;
                            Log.Error($"Error while trying to parse release time ({released}) of nitrox v{version}");
                        }

                        builder.Clear();
                        for (int j = 0; j < patchnotes.Count; j++)
                        {
                            builder.AppendLine($"• {(string)patchnotes[j]}");
                        }

                        changelogs.Add(new NitroxChangelog(version, dateTime, builder.ToString()));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(Downloader)} : Error while fetching nitrox changelogs from {CHANGELOGS_URL}");
                LauncherNotifier.Error("Unable to fetch nitrox changelogs");
            }

            return changelogs;
        }

        public async static Task<Version> GetNitroxLatestVersion()
        {
            try
            {
                using WebResponse response = await GetResponseFromCache(LATEST_VERSION_URL);

                if (response.IsFromCache)
                {
                    Log.Info("Fetched nitrox version from the local cache");
                }

                using (StreamReader sr = new(response.GetResponseStream()))
                {
                    string json = await sr.ReadToEndAsync();
                    Regex rx = new(@"""version"":""([^""]*)""");
                    Match match = rx.Match(json);

                    if (match.Success && match.Groups.Count > 1)
                    {
                        return new Version(match.Groups[1].Value);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(Downloader)} : Error while fetching nitrox version from {LATEST_VERSION_URL}");
                LauncherNotifier.Error("Unable to check for updates");
            }

            return new Version();
        }

        private static async Task<WebResponse> GetResponseFromCache(string url)
        {
            Log.Info($"Trying to request data from {url}");

            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "GET";
            request.UserAgent = "NitroxLauncher";
            request.ContentType = "application/json";
            request.CachePolicy = cachePolicy;
            request.Timeout = 5000;

            try
            {
                return await request.GetResponseAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error while requesting data from {url}");
            }

            return null;
        }
    }
}
