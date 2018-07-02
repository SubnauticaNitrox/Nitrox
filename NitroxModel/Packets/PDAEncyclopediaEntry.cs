using System;

namespace NitroxModel.Packets
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
