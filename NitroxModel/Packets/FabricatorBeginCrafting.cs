using NitroxModel.DataStructures;
using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class FabricatorBeginCrafting : AuthenticatedPacket
    {
        public String FabricatorGuid { get; }
        public TechType TechType { get { return serializableTechType.TechType; } }
        public float Duration { get; }

        private SerializableTechType serializableTechType;

        public FabricatorBeginCrafting(String playerId, String fabricatorGuid, TechType techType, float duration) : base(playerId)
        {
            this.FabricatorGuid = fabricatorGuid;
            this.serializableTechType = new SerializableTechType(techType);
            this.Duration = duration;
        }

        public override string ToString()
        {
            return "[FabricatorBeginCrafting - FabricatorGuid: " + FabricatorGuid + " TechType: " + TechType + " Duration: " + Duration + "]";
        }
    }
}
