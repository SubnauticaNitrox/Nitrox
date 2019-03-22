using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CrafterEndUse : Packet
    {
        public string CrafterGuid { get; }

        public CrafterEndUse(string crafterGuid)
        {
            CrafterGuid = crafterGuid;
        }

        public override string ToString()
        {
            return "[CrafterEndUse - CrafterGuid: " + CrafterGuid + "]";
        }
    }
}
