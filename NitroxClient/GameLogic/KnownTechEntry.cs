using NitroxClient.Communication.Abstract;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.GameLogic
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
