using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class KnownTechEntryAdd : Packet
    {
        public enum EntryCategory
        {
            KNOWN = 0,
            ANALYZED = 1
        }

        public NitroxTechType TechType { get; }
        public bool Verbose { get; }
        public EntryCategory Category { get; }

        public KnownTechEntryAdd(EntryCategory category, NitroxTechType techType, bool verbose)
        {
            Category = category;
            TechType = techType;
            Verbose = verbose;
        }
    }
}
