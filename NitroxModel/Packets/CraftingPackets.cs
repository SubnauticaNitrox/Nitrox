using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CrafterBeginCrafting : Packet
    {
        public string CrafterGuid { get; }
        public TechType TechType { get; }
        public float Duration { get; }

        public CrafterBeginCrafting(string crafterGuid, TechType techType, float duration)
        {
            CrafterGuid = crafterGuid;
            TechType = techType;
            Duration = duration;
        }

        public override string ToString()
        {
            return "[CrafterBeginCrafting - CrafterGuid: " + CrafterGuid + " TechType: " + TechType + " Duration: " + Duration + "]";
        }
    }

    [Serializable]
    public class CrafterItemPickup : Packet
    {
        public string CrafterGuid { get; }

        public CrafterItemPickup(string crafterGuid)
        {
            CrafterGuid = crafterGuid;
        }

        public override string ToString()
        {
            return "[CrafterItemPickup - CrafterGuid: " + CrafterGuid + "]";
        }
    }

    [Serializable]
    public class CrafterEndCrafting : Packet
    {
        public string CrafterGuid { get; }

        public CrafterEndCrafting(string crafterGuid)
        {
            CrafterGuid = crafterGuid;
        }

        public override string ToString()
        {
            return "[CrafterEndCrafting - CrafterGuid: " + CrafterGuid + "]";
        }
    }
}
