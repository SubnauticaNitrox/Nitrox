using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CrafterStartUse : Packet
    {
        public string PlayerName { get; }
        public string CrafterGuid { get; }
        
        public CrafterStartUse(string playerName, string crafterGuid)
        {
            PlayerName = playerName;
            CrafterGuid = crafterGuid;
        }
    }
}
