using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;

namespace Nitrox.Model.Packets
{
    [Serializable]
    public class FabricatorBeginCrafting : Packet
    {
        public NitroxId FabricatorId { get; }
        public NitroxTechType TechType { get; }
        public float Duration { get; }

        public FabricatorBeginCrafting(NitroxId fabricatorId, NitroxTechType techType, float duration)
        {
            FabricatorId = fabricatorId;
            TechType = techType;
            Duration = duration;
        }

        public override string ToString()
        {
            return "[FabricatorBeginCrafting - FabricatorId: " + FabricatorId + " TechType: " + TechType + " Duration: " + Duration + "]";
        }
    }
}
