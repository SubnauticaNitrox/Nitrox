using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ConstructorBeginCrafting : Packet
    {
        public String Guid { get; protected set; }
        public String TechType { get; protected set; }
        public float Duration { get; protected set; }

        public ConstructorBeginCrafting(String playerId, String guid, String techType, float duration) : base(playerId)
        {
            this.Guid = guid;
            this.TechType = techType;
            this.Duration = duration;
        }

        public override string ToString()
        {
            return "[ConstructorBeginCrafting - Guid: " + Guid + " TechType: " + TechType + " Duration: " + Duration + "]";
        }
    }
}
