using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace NitroxModel.Helper
{
    public static class WebHelper
    {
        //"Hacky way" to get the public IP
        //Should definitely be reworked.
        public static string GetPublicIP()
        {
            WebRequest req = WebRequest.Create("http://checkip.dyndns.org");

            using (StreamReader sr = new StreamReader(req.GetResponse().GetResponseStream()))
            {
                return sr.ReadToEnd().Trim().Split(':')[1].Substring(1).Split('<')[0];
            }
        }

        public static string GetNitroxLatestVersion()
        {
            HttpWebRequest req = WebRequest.Create("https://api.github.com/repos/SubnauticaNitrox/Nitrox/releases/latest") as HttpWebRequest;
            req.Method = "GET";
            req.UserAgent = "Nitrox";
            req.ContentType = "application/json";
            req.Timeout = 1000;

            string version = string.Empty;

            try
            {
                using (StreamReader sr = new StreamReader(req.GetResponse().GetResponseStream()))
                {
                    string json = sr.ReadToEnd();
                    Regex rx = new Regex("\\\"name\\\":\\\"(([^\\\"])*)\\\"");
                    Match match = rx.Match(json);

                    if (match.Success)
                    {
                        version = match.Groups[1].Value;
                    }
                }
            }
            catch
            {
            }

            return version;
        }
    }
}
