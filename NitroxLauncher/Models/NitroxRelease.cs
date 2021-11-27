using System;

namespace NitroxLauncher.Models
{
    [Serializable]
    internal class NitroxRelease
    {
        public string Url { get; set; }

        public string Version { get; set; }

        public string FileSize { get; set; }

        public string Md5 { get; set; }

        protected NitroxRelease()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public NitroxRelease(string url, string version, string filesize, string md5)
        {
            Url = url;
            Version = version;
            FileSize = filesize;
            Md5 = md5;
        }

        public override string ToString()
        {
            return $"[{nameof(NitroxRelease)} - Url: {Url}, Version: {Version}, FileSize: {FileSize}, Md5: {Md5}]";
        }
    }
}
