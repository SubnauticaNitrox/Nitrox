using System;

namespace NitroxLauncher.Models
{
    [Serializable]
    internal class NitroxChangelog
    {
        public string Version { get; set; }

        public DateTime Released { get; set; }

        public string PatchNotes { get; set; }

        protected NitroxChangelog()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public NitroxChangelog(string version, DateTime released, string patchnotes)
        {
            Version = version;
            Released = released;
            PatchNotes = patchnotes;
        }

        public override string ToString()
        {
            return $"[{nameof(NitroxChangelog)} - Version: {Version}, Released: {Released}, PatchNotes: {PatchNotes}]";
        }
    }
}
