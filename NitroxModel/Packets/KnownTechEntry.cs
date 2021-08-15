using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
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
    }
}
