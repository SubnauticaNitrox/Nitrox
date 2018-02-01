using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsBeginSilentRunningProcessor : ClientPacketProcessor<CyclopsBeginSilentRunning>
    {
        private readonly IPacketSender packetSender;

        public CyclopsBeginSilentRunningProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CyclopsBeginSilentRunning packet)
        {
            GameObject cyclops = GuidHelper.RequireObjectFrom(packet.Guid);
            CyclopsSilentRunningAbilityButton ability = cyclops.RequireComponentInChildren<CyclopsSilentRunningAbilityButton>();
            
            using (packetSender.Suppress<CyclopsBeginSilentRunning>())
            {
                ability.subRoot.BroadcastMessage("RigForSilentRunning");
                ability.InvokeRepeating("SilentRunningIteration", 0f, ability.silentRunningIteration);
            }
        }
    }
}
