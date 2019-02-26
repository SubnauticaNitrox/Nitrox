using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel_Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    class CyclopsDecoyLaunchProcessor : ClientPacketProcessor<CyclopsDecoyLaunch>
    {
        private readonly IPacketSender packetSender;

        public CyclopsDecoyLaunchProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CyclopsDecoyLaunch decoyLaunchPacket)
        {
            GameObject cyclops = GuidHelper.RequireObjectFrom(decoyLaunchPacket.Guid);
            CyclopsDecoyManager decoyManager = cyclops.RequireComponent<CyclopsDecoyManager>();

            if(decoyManager.TryLaunchDecoy())
            {
                CyclopsDecoyLaunchButton decoyLaunchButton = cyclops.RequireComponent<CyclopsDecoyLaunchButton>();
                decoyLaunchButton.StartCooldown();
            }
        }
    }
}
