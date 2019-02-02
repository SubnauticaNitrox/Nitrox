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
    public class PDAEncyclopediaEntryAddProcessor : ClientPacketProcessor<PDAEncyclopediaEntryAdd>
    {
        private readonly IPacketSender packetSender;

        public PDAEncyclopediaEntryAddProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(PDAEncyclopediaEntryAdd packet)
        {
            using (packetSender.Suppress<PDAEncyclopediaEntryAdd>())
            {
                if (!packet.Entry.IsTimeCapsule)
                {
                    PDAEncyclopedia.Add(packet.Entry.Key, true);
                }
                else
                {
                    PDAEncyclopedia.AddTimeCapsule(packet.Entry.Key, true);
                }
            }
        }
    }
}
