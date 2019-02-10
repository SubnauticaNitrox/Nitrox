using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class FabricatorBeginCrafting : Packet
    {
        public string FabricatorGuid { get; }
        public TechType TechType { get; }
        public float Duration { get; }

        public FabricatorBeginCrafting(string fabricatorGuid, TechType techType, float duration)
        {
            FabricatorGuid = fabricatorGuid;
            TechType = techType;
            Duration = duration;
        }

        public override string ToString()
        {
            return "[FabricatorBeginCrafting - FabricatorGuid: " + FabricatorGuid + " TechType: " + TechType + " Duration: " + Duration + "]";
        }
    }
}
