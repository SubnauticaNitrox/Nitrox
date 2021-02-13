using NitroxClient.Communication.Abstract;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic
{
    public class PDAEncyclopediaEntry
    {
        private readonly IPacketSender packetSender;

        public PDAEncyclopediaEntry(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void Add(string key, bool postNotification)
        {
            PDAEncyclopediaEntryAdd EntryAdd = new PDAEncyclopediaEntryAdd(key, postNotification);
            packetSender.Send(EntryAdd);
        }
    }

}
