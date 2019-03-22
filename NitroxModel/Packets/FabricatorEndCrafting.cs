using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class FabricatorEndCrafting : Packet
    {
        public string FabricatorGuid { get; }

        public FabricatorEndCrafting(string fabricatorGuid)
        {
            FabricatorGuid = fabricatorGuid;
        }

        public override string ToString()
        {
            return "[FabricatorEndCrafting - FabricatorGuid: " + FabricatorGuid + "]";
        }
    }
}
