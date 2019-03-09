using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;
using NitroxModel_Subnautica.Packets;
using UnityEngine;


namespace NitroxClient.Communication.Packets.Processors
{
    class CyclopsSonarPingProcessor : ClientPacketProcessor<CyclopsSonarPing>
    {
        private readonly IPacketSender packetSender;

        public CyclopsSonarPingProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CyclopsSonarPing sonarPacket)
        {
            GameObject cyclops = GuidHelper.RequireObjectFrom(sonarPacket.Guid);
            CyclopsSonarButton sonar = cyclops.RequireComponentInChildren<CyclopsSonarButton>();
            using (packetSender.Suppress<CyclopsSonarPing>())
            {
                sonar.ReflectionCall("SonarPing");
            }
        }
    }
}
