﻿using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsActivateShieldProcessor : ClientPacketProcessor<CyclopsActivateShield>
    {
        private readonly IPacketSender packetSender;

        public CyclopsActivateShieldProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CyclopsActivateShield shieldPacket)
        {
            GameObject cyclops = GuidHelper.RequireObjectFrom(shieldPacket.Guid);
            CyclopsShieldButton shield = cyclops.RequireComponentInChildren<CyclopsShieldButton>();

            using (packetSender.Suppress<CyclopsActivateShield>())
            {
                shield.OnClick();
            }
        }
    }
}
