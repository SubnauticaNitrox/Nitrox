using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class FabricatorItemPickup : Packet
    {
        public NitroxId FabricatorId { get; }
        public TechType TechType { get; }

        public FabricatorItemPickup(NitroxId fabricatorId, TechType techType)
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
