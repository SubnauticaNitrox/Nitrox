using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class FabricatorItemPickup : AuthenticatedPacket
    {
        public String FabricatorGuid { get; protected set; }
        public String TechType { get; protected set; }

        public FabricatorItemPickup(String playerId, String fabricatorGuid, String techType) : base(playerId)
        {
            this.FabricatorGuid = fabricatorGuid;
            this.TechType = techType;
        }

        public override string ToString()
        {
            return "[FabricatorItemPickup - FabricatorGuid: " + FabricatorGuid + " TechType: " + TechType + "]";
        }
    }
}
