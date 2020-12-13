using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;

namespace Nitrox.Model.Packets
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
