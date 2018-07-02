using System;
using System.Collections.Generic;
using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.Packets;
using Story;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class KnownTechEntryProcessorAdd : ClientPacketProcessor<KnownTechEntryAdd>
    {
        private readonly IPacketSender packetSender;

        public KnownTechEntryProcessorAdd(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(KnownTechEntryAdd packet)
        {
            using (packetSender.Suppress<KnownTechEntryAdd>())
            using (packetSender.Suppress<KnownTechEntryChanged>())
            {
                KnownTech.Add(packet.TechType, packet.Verbose);
            }
        }
    }
}
