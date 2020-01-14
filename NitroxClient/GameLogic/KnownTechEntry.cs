﻿using NitroxClient.Communication.Abstract;
using NitroxModel.Packets;
using NitroxModel_Subnautica.Helper;

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
            KnownTechEntryAdd EntryAdd = new KnownTechEntryAdd(techType.Model(), verbose);
            packetSender.Send(EntryAdd);
        }
    }
}
