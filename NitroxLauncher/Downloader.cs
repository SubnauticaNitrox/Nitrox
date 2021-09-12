using System;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NitroxModel.Logger;

namespace NitroxLauncher
{
    internal static class Downloader
    {
        public const string LATEST_VERSION_URL = "https://nitrox.rux.gg/api/version/latest";
        public const string CHANGELOG_URL = " https://nitrox.rux.gg/api/changelog/releases";
        public const string RELEASES_URL = " https://nitrox.rux.gg/api/version/releases";
        public const string BLOGS_URL = " https://nitroxblog.rux.gg/wp-json/wp/v2/posts";

        public async static Task<Version> GetNitroxLatestVersion()
        {
            WebResponse response = await GetResponseFromCache(LATEST_VERSION_URL);

            try
            {
                using (StreamReader sr = new(response.GetResponseStream()))
                {
                    string json = await sr.ReadToEndAsync();
                    Regex rx = new(@"""version"":""([^""]*)""");
                    Match match = rx.Match(json);

                    if (match.Success)
                    {
                        return new Version(match.Groups[1].Value);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "WebHelper : Error while fetching nitrox latest version on GitHub");
            }

            return new Version();
        }

        public async static Task<WebResponse> GetResponseFromCache(string url)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "GET";
            request.UserAgent = "NitroxLauncher";
            request.ContentType = "application/json";
            request.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.CacheIfAvailable);
            request.Timeout = 5000;

            try
            {
                return await request.GetResponseAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Downloader : Error while fetching content");
            }

            return null;
        }
    }
}
