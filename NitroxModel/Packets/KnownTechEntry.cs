using System;
using DTO = NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class KnownTechEntryAdd : Packet
    {
        public DTO.TechType TechType;
        public bool Verbose;

        public KnownTechEntryAdd(DTO.TechType techType, bool verbose)
        {
            TechType = techType;
            Verbose = verbose;
        }

        public override string ToString()
        {
            return $"[{nameof(KnownTechEntryAdd)} - TechType: " + TechType + " Verbose: " + Verbose + "]";
        }
    }
}
