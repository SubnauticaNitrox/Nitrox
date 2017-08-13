using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class FabricatorBeginCrafting : AuthenticatedPacket
    {
        public String FabricatorGuid { get; protected set; }
        public String TechType { get; protected set; }
        public float Duration { get; protected set; }

        public FabricatorBeginCrafting(String playerId, String fabricatorGuid, String techType, float duration) : base(playerId)
        {
            this.FabricatorGuid = fabricatorGuid;
            this.TechType = techType;
            this.Duration = duration;
        }

        public override string ToString()
        {
            return "[FabricatorBeginCrafting - FabricatorGuid: " + FabricatorGuid + " TechType: " + TechType + " Duration: " + Duration + "]";
        }
    }
}
