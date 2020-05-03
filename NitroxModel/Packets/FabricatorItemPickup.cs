using System;
using DTO = NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class FabricatorItemPickup : Packet
    {
        public DTO.NitroxId FabricatorId { get; }
        public DTO.TechType TechType { get; }

        public FabricatorItemPickup(DTO.NitroxId fabricatorId, DTO.TechType techType)
        {
            FabricatorId = fabricatorId;
            TechType = techType;
        }

        public override string ToString()
        {
            return "[FabricatorItemPickup - FabricatorId: " + FabricatorId + " TechType: " + TechType + "]";
        }
    }
}
