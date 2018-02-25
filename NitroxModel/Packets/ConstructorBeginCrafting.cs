using System;
using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ConstructorBeginCrafting : Packet
    {
        public string ConstructorGuid { get; }
        public string ConstructedItemGuid { get; }
        public TechType TechType { get; }
        public float Duration { get; }
        public List<InteractiveChildObjectIdentifier> InteractiveChildIdentifiers { get; }

        public ConstructorBeginCrafting(string constructorGuid, string constructedItemGuid, TechType techType, float duration, List<InteractiveChildObjectIdentifier> interactiveChildIdentifiers)
        {
            ConstructorGuid = constructorGuid;
            ConstructedItemGuid = constructedItemGuid;
            TechType = techType;
            Duration = duration;
            InteractiveChildIdentifiers = interactiveChildIdentifiers;
        }

        public override string ToString()
        {
            string s = "[ConstructorBeginCrafting - ConstructorGuid: " + ConstructorGuid + " ConstructedItemGuid: " + ConstructedItemGuid + " TechType: " + TechType + " Duration: " + Duration + " InteractiveChildIdentifiers: (";

            foreach (InteractiveChildObjectIdentifier childIdentifier in InteractiveChildIdentifiers)
            {
                s += childIdentifier + " ";
            }

            return s + ")";
        }
    }
}
