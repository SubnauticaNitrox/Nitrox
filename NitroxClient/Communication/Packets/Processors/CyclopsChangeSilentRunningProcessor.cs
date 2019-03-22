﻿using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;
using NitroxModel_Subnautica.Packets;
using UnityEngine;
using System.Reflection;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsChangeSilentRunningProcessor : ClientPacketProcessor<CyclopsChangeSilentRunning>
    {
        private readonly IPacketSender packetSender;

        public CyclopsChangeSilentRunningProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CyclopsChangeSilentRunning packet)
        {
            GameObject cyclops = GuidHelper.RequireObjectFrom(packet.Guid);
            CyclopsSilentRunningAbilityButton ability = cyclops.RequireComponentInChildren<CyclopsSilentRunningAbilityButton>();

            using (packetSender.Suppress<CyclopsChangeSilentRunning>())
            {
                ability.ReflectionSet("active", packet.IsOn);
                if (packet.IsOn)
                {
                    ability.image.sprite = ability.activeSprite;
                    ability.subRoot.BroadcastMessage("RigForSilentRunning");
                    ability.InvokeRepeating("SilentRunningIteration", 0f, ability.silentRunningIteration);
                } else
                {
                    ability.image.sprite = ability.inactiveSprite;
                    ability.subRoot.BroadcastMessage("SecureFromSilentRunning");
                    ability.CancelInvoke("SilentRunningIteration");
                }
            }
        }
    }
}
