using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class FabricatorBeginCrafting : AuthenticatedPacket
    {
        public string FabricatorGuid { get; }
        public TechType TechType { get; }
        public float Duration { get; }

        public FabricatorBeginCrafting(string playerId, string fabricatorGuid, TechType techType, float duration) : base(playerId)
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
