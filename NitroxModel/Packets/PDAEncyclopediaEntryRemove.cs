using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PDAEncyclopediaEntryRemove : Packet
    {
        public EncyclopediaEntry Entry;

        public PDAEncyclopediaEntryRemove(EncyclopediaEntry entry)
        {
            Entry = entry;
        }

        public override string ToString()
        {
            return "[PDAEncyclopediaEntryRemove - " + Entry + "]";
        }
    }
}
