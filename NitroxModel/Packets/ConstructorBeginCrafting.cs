using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ConstructorBeginCrafting : Packet
    {
        public String ConstructorGuid { get; protected set; }
        public String ConstructedItemGuid { get; protected set; }
        public String TechType { get; protected set; }
        public float Duration { get; protected set; }

        public ConstructorBeginCrafting(String playerId, String constructorGuid, String constructedItemGuid, String techType, float duration) : base(playerId)
        {
            this.ConstructorGuid = constructorGuid;
            this.ConstructedItemGuid = constructedItemGuid;
            this.TechType = techType;
            this.Duration = duration;
        }

        public override string ToString()
        {
            return "[ConstructorBeginCrafting - ConstructorGuid: " + ConstructorGuid + " ConstructedItemGuid: " + ConstructedItemGuid + " TechType: " + TechType + " Duration: " + Duration + "]";
        }
    }
}
