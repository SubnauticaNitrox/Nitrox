using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class FabricatorItemPickup : Packet
    {
        public NitroxId FabricatorId { get; }
        public DataStructures.TechType TechType { get; }

        public FabricatorItemPickup(NitroxId fabricatorId, DataStructures.TechType techType)
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
