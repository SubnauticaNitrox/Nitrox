using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper.GameLogic;
using NitroxModel.Packets;
using System;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsBeginSilentRunningProcessor : ClientPacketProcessor<CyclopsBeginSilentRunning>
    {
        private PacketSender packetSender;

        public CyclopsBeginSilentRunningProcessor(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CyclopsBeginSilentRunning packet)
        {
            Optional<GameObject> opCyclops = GuidHelper.GetObjectFrom(packet.Guid);

            if (opCyclops.IsPresent())
            {
                CyclopsSilentRunningAbilityButton ability = opCyclops.Get().GetComponentInChildren<CyclopsSilentRunningAbilityButton>();

                if (ability != null)
                {
                    using (packetSender.Suppress<CyclopsBeginSilentRunning>())
                    {
                        ability.subRoot.BroadcastMessage("RigForSilentRunning");
                        ability.InvokeRepeating("SilentRunningIteration", 0f, ability.silentRunningIteration);
                    }
                }
                else
                {
                    Console.WriteLine("Could not begin silent running because CyclopsSilentRunningAbilityButton was not found on the cyclops " + packet.Guid);
                }
            }
            else
            {
                Console.WriteLine("Could not find cyclops with guid " + packet.Guid + " to begin silent running.");
            }
        }
    }
}
