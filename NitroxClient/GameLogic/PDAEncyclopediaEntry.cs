using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class PDAEncyclopediaEntry
    {
        private readonly IPacketSender packetSender;

        public PDAEncyclopediaEntry(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void Add(string key)
        {
            PDAEncyclopediaEntryAdd EntryAdd = new PDAEncyclopediaEntryAdd(key);
            packetSender.Send(EntryAdd);
        }

        public void Update(string key)
        {
            PDAEncyclopediaEntryUpdate EntryUpdate = new PDAEncyclopediaEntryUpdate(key);
            packetSender.Send(EntryUpdate);
        }

        public void Remove(string key)
        {
            PDAEncyclopediaEntryRemove EntryRemoved = new PDAEncyclopediaEntryRemove(key);
            packetSender.Send(EntryRemoved);
        }
    }

}
