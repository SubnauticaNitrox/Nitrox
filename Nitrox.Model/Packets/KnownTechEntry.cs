using System;
using Nitrox.Model.DataStructures.GameLogic;

namespace Nitrox.Model.Packets
{
    [Serializable]
    public class KnownTechEntryAdd : Packet
    {
        public NitroxTechType TechType { get; }
        public bool Verbose { get; }

        public KnownTechEntryAdd(NitroxTechType techType, bool verbose)
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
