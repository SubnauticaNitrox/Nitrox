using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PDAEncyclopediaEntryRemove : Packet
    {
        public string Key;

        public PDAEncyclopediaEntryRemove(string key)
        {
            Key = key;
        }

        public override string ToString()
        {
            return "[PDAEncyclopediaEntryRemove - Key: " + Key + "]";
        }
    }
}
