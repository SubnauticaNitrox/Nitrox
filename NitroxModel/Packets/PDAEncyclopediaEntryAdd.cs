using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PDAEncyclopediaEntryAdd : Packet
    {
        public EncyclopediaEntry Entry;

        public PDAEncyclopediaEntryAdd(EncyclopediaEntry entry)
        {
            Entry = entry;
        }

        public override string ToString()
        {
            return "[PDAEncyclopediaEntryAdd - " + Entry + "]";
        }
    }
}
