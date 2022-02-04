﻿using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PDALogEntryAddProcessor : ClientPacketProcessor<PDALogEntryAdd>
    {
        private readonly IPacketSender packetSender;

        public PDALogEntryAddProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(PDALogEntryAdd packet)
        {
            using (packetSender.Suppress<PDALogEntryAdd>())
            {
                PDALog.Add(packet.Key);
            }
        }
    }
}
