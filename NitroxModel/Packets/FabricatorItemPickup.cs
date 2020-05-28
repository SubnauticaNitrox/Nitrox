using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class FabricatorItemPickup : Packet
    {
        public NitroxId FabricatorId { get; }
        public NitroxTechType TechType { get; }

        public FabricatorItemPickup(NitroxId fabricatorId, NitroxTechType techType)
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
