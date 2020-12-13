using Nitrox.Client.Communication.Abstract;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures;

namespace Nitrox.Client.GameLogic
{
    public class KnownTechEntry
    {
        private readonly IPacketSender packetSender;

        public KnownTechEntry(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void Add(TechType techType, bool verbose)
        {
            KnownTechEntryAdd EntryAdd = new KnownTechEntryAdd(techType.ToDto(), verbose);
            packetSender.Send(EntryAdd);
        }
    }
}
