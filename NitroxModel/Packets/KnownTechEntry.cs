using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class KnownTechEntryAdd : Packet
    {
        public NitroxModel.DataStructures.TechType TechType { get; }
        public bool Verbose { get; }

        public KnownTechEntryAdd(NitroxModel.DataStructures.TechType techType, bool verbose)
        {
            TechType = techType;
            Verbose = verbose;
        }
        public override string ToString()
        {
            return "[KnownTechEntryAdd - TechType: " + TechType + " Verbose: " + Verbose + "]";
        }
    }    
}
