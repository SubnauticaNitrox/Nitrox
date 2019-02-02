using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxClient.GameLogic
{
    public class PDAEncyclopediaEntry
    {
        private readonly IPacketSender packetSender;

        public PDAEncyclopediaEntry(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void Add(EncyclopediaEntry entry)
        {
            PDAEncyclopediaEntryAdd EntryAdd = new PDAEncyclopediaEntryAdd(entry);
            packetSender.Send(EntryAdd);
        }

        public void Update(EncyclopediaEntry entry)
        {
            PDAEncyclopediaEntryUpdate EntryUpdate = new PDAEncyclopediaEntryUpdate(entry);
            packetSender.Send(EntryUpdate);
        }

        public void Remove(EncyclopediaEntry entry)
        {
            PDAEncyclopediaEntryRemove EntryRemoved = new PDAEncyclopediaEntryRemove(entry);
            packetSender.Send(EntryRemoved);
        }
    }

}
