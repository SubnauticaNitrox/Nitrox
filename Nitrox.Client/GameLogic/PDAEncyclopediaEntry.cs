using Nitrox.Client.Communication.Abstract;
using Nitrox.Model.Packets;

namespace Nitrox.Client.GameLogic
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
    }

}
