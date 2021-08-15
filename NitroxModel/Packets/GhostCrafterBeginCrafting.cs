using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class GhostCrafterBeginCrafting : Packet
    {
        public NitroxId GhostCrafterId { get; }
        public NitroxTechType TechType { get; }
        public float Duration { get; }

        public GhostCrafterBeginCrafting(NitroxId ghostCrafterId, NitroxTechType techType, float duration)
        {
            GhostCrafterId = ghostCrafterId;
            TechType = techType;
            Duration = duration;
        }
    }
}
