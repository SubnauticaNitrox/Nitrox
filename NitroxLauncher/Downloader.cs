using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LitJson;
using NitroxLauncher.Models;
using NitroxModel.Logger;

namespace NitroxLauncher
{
    internal static class Downloader
    {
        public const string LATEST_VERSION_URL = "https://nitrox.rux.gg/api/version/latest";
        public const string CHANGELOGS_URL = "https://nitrox.rux.gg/api/changelog/releases";
        public const string RELEASES_URL = " https://nitrox.rux.gg/api/version/releases";
        public const string BLOGS_URL = " https://nitroxblog.rux.gg/wp-json/wp/v2/posts";
        public const string LATEST_RELEASE_URL = "https://nitrox.rux.gg/download";

        // Create a policy that allows any cache to supply requested resources if the resource on the server is not newer than the cached copy
        private static readonly HttpRequestCachePolicy cachePolicy = new(
            HttpCacheAgeControl.MaxAge,
            TimeSpan.FromHours(2)
        );

        public static Task<IList<NitroxBlog>> GetBlogs()
        {
            throw new NotImplementedException();
        }

        public async static Task<IList<NitroxChangelog>> GetChangeLogs()
        {
            IList<NitroxChangelog> changelogs = new List<NitroxChangelog>();

            try
            {
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
            }

            return new Version();
        }

        private async static Task<WebResponse> GetResponseFromCache(string url)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                Log.Warn("Launcher might not be connected to internet");
            }

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
