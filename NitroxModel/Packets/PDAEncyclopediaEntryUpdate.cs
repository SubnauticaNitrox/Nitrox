using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PDAEncyclopediaEntryUpdate : Packet
    {
        public string Key;

        public PDAEncyclopediaEntryUpdate(string key)
        {
            Key = key;
        }

        public override string ToString()
        {
            return "[PDAEncyclopediaEntryUpdate - Key: " + Key + "]";
        }
    }
}
