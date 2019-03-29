using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using NitroxModel.Helper;
using NitroxModel_Subnautica.Packets;
using UnityEngine;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsFireSuppressionProcessor : ClientPacketProcessor<CyclopsFireSuppression>
    {
        private readonly IPacketSender packetSender;
        private readonly Cyclops cyclops;

        public CyclopsFireSuppressionProcessor(IPacketSender packetSender, Cyclops cyclops)
        {
            this.packetSender = packetSender;
            this.cyclops = cyclops;
        }

        public override void Process(CyclopsFireSuppression fireSuppressionPacket)
        {
            cyclops.StartFireSuppression(fireSuppressionPacket.Guid);
        }
    }
}
