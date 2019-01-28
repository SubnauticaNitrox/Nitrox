using System.IO;
using System.Net;

namespace NitroxModel.Helper
{
    public static class IPHelper
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
    }
}
