using System;
using DTO = NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class FabricatorBeginCrafting : Packet
    {
        public DTO.NitroxId FabricatorId { get; }
        public DTO.TechType TechType { get; }
        public float Duration { get; }

        public FabricatorBeginCrafting(DTO.NitroxId fabricatorId, DTO.TechType techType, float duration)
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
