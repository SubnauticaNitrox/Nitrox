using NitroxClient.Communication.Abstract;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class KnownTechEntry
    {
        private readonly IPacketSender packetSender;

        public KnownTechEntry(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void AddKnown(TechType techType, bool verbose)
        {
            packetSender.Send(new KnownTechEntryAdd(KnownTechEntryAdd.EntryCategory.KNOWN, techType.ToDto(), verbose));
        }

        public void AddAnalyzed(TechType techType, bool verbose)
        {
            packetSender.Send(new KnownTechEntryAdd(KnownTechEntryAdd.EntryCategory.ANALYZED, techType.ToDto(), verbose));
        }
    }
}
