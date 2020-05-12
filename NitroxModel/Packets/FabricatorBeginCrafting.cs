using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class FabricatorBeginCrafting : Packet
    {
        public NitroxId FabricatorId { get; }
        public DataStructures.TechType TechType { get; }
        public float Duration { get; }

        public FabricatorBeginCrafting(NitroxId fabricatorId, DataStructures.TechType techType, float duration)
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
