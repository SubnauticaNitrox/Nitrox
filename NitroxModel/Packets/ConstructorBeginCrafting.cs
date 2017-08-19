using NitroxModel.DataStructures;
using System;
using System.Collections.Generic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ConstructorBeginCrafting : AuthenticatedPacket
    {
        public String ConstructorGuid { get; protected set; }
        public String ConstructedItemGuid { get; protected set; }
        public String TechType { get; protected set; }
        public float Duration { get; protected set; }
        public List<InteractiveChildObjectIdentifier> InteractiveChildIdentifiers { get; private set; }

        public ConstructorBeginCrafting(String playerId, String constructorGuid, String constructedItemGuid, String techType, float duration, List<InteractiveChildObjectIdentifier> interactiveChildIdentifiers) : base(playerId)
        {
            this.ConstructorGuid = constructorGuid;
            this.ConstructedItemGuid = constructedItemGuid;
            this.TechType = techType;
            this.Duration = duration;
            this.InteractiveChildIdentifiers = interactiveChildIdentifiers;
        }

        public override string ToString()
        {
            String s = "[ConstructorBeginCrafting - ConstructorGuid: " + ConstructorGuid + " ConstructedItemGuid: " + ConstructedItemGuid + " TechType: " + TechType + " Duration: " + Duration + " InteractiveChildIdentifiers: (";

            foreach(InteractiveChildObjectIdentifier childIdentifier in InteractiveChildIdentifiers)
            {
                s += childIdentifier + " ";
            }

            return s + ")";
        }
    }
}
