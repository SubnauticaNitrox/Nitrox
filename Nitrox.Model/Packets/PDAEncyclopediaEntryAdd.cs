using System;

namespace Nitrox.Model.Packets
{
    [Serializable]
    public class PDAEncyclopediaEntryAdd : Packet
    {
        public string Key;

        public PDAEncyclopediaEntryAdd(string key)
        {
            Key = key;
        }

        public override string ToString()
        {
            return "[PDAEncyclopediaEntryAdd - Key: " + Key + "]";
        }
    }
}
