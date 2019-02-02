using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PDAEncyclopediaEntryUpdate : Packet
    {
        public EncyclopediaEntry Entry;

        public PDAEncyclopediaEntryUpdate(EncyclopediaEntry entry)
        {
            Entry = entry;
        }

        public override string ToString()
        {
            return "[PDAEncyclopediaEntryUpdate - " + Entry + "]";
        }
    }
}
