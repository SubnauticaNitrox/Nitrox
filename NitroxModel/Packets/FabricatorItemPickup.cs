using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class FabricatorItemPickup : AuthenticatedPacket
    {
        public string FabricatorGuid { get; }
        public TechType TechType { get; }

        public FabricatorItemPickup(string playerId, string fabricatorGuid, TechType techType) : base(playerId)
        {
            FabricatorGuid = fabricatorGuid;
            TechType = techType;
        }

        public override string ToString()
        {
            return "[FabricatorItemPickup - FabricatorGuid: " + FabricatorGuid + " TechType: " + TechType + "]";
        }
    }
}
