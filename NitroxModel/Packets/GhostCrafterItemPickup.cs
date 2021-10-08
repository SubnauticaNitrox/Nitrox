using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class GhostCrafterItemPickup : Packet
    {
        public NitroxId GhostCrafterId { get; }
        public NitroxTechType TechType { get; }

        public GhostCrafterItemPickup(NitroxId ghostCrafterId, NitroxTechType techType)
        {
            GhostCrafterId = ghostCrafterId;
            TechType = techType;
        }
    }
}
