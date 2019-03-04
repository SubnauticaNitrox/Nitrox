using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;
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
            using (packetSender.Suppress<CyclopsChangeSilentRunning>())
            {
                if (decoyManager.decoyCount > 0)
                {
                    decoyManager.Invoke("LaunchWithDelay", 3f);
                    decoyManager.decoyLaunchButton.UpdateText();
                    decoyManager.subRoot.voiceNotificationManager.PlayVoiceNotification(decoyManager.subRoot.decoyNotification, false, true);
                    decoyManager.subRoot.BroadcastMessage("UpdateTotalDecoys", decoyManager.decoyCount, SendMessageOptions.DontRequireReceiver);
                    CyclopsDecoyLaunchButton decoyLaunchButton = cyclops.RequireComponent<CyclopsDecoyLaunchButton>();
                    decoyLaunchButton.StartCooldown();
                }
            }
        }
    }
}
