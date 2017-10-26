using System;
using System.Collections.Generic;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ConstructorBeginCrafting : AuthenticatedPacket
    {
        public String ConstructorGuid { get; }
        public String ConstructedItemGuid { get; }
        public TechType TechType { get; }
        public float Duration { get; }
        public List<InteractiveChildObjectIdentifier> InteractiveChildIdentifiers { get; }

        public ConstructorBeginCrafting(String playerId, String constructorGuid, String constructedItemGuid, TechType techType, float duration, List<InteractiveChildObjectIdentifier> interactiveChildIdentifiers) : base(playerId)
        {
            ConstructorGuid = constructorGuid;
            ConstructedItemGuid = constructedItemGuid;
            TechType = techType;
            Duration = duration;
            InteractiveChildIdentifiers = interactiveChildIdentifiers;
        }

        public override string ToString()
        {
            String s = "[ConstructorBeginCrafting - ConstructorGuid: " + ConstructorGuid + " ConstructedItemGuid: " + ConstructedItemGuid + " TechType: " + TechType + " Duration: " + Duration + " InteractiveChildIdentifiers: (";

            foreach (InteractiveChildObjectIdentifier childIdentifier in InteractiveChildIdentifiers)
            {
                s += childIdentifier + " ";
            }

            return s + ")";
        }
    }
}
