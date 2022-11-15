using System;

namespace NitroxLauncher.Models
{
    [Serializable]
    internal class NitroxChangelog
    {
        public string Version { get; }

        public DateTime Released { get; }

        public string PatchNotes { get; }

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
