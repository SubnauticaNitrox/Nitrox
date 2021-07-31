using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PDAEncyclopediaEntryAdd : Packet
    {
        public string Key { get; }

        public PDAEncyclopediaEntryAdd(string key)
        {
            Key = key;
        }
    }
}
