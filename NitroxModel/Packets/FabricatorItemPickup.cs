using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class FabricatorItemPickup : Packet
    {
        public string FabricatorGuid { get; }
        public TechType TechType { get; }

        public FabricatorItemPickup(string fabricatorGuid, TechType techType)
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
