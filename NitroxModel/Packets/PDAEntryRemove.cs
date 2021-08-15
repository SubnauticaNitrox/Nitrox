using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PDAEntryRemove : Packet
    {
        public NitroxTechType TechType { get; }

        public PDAEntryRemove(NitroxTechType techType)
        {
            TechType = techType;
        }
    }
}
