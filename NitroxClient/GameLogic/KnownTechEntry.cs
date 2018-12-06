using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
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

        public void Add(TechType techType, bool verbose)
        {
            KnownTechEntryAdd EntryAdd = new KnownTechEntryAdd(techType, verbose);
            packetSender.Send(EntryAdd);
        }
    }
}
