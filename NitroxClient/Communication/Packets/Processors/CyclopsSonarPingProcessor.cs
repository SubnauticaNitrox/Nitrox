using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
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
        private readonly Cyclops cyclops;

        public CyclopsSonarPingProcessor(IPacketSender packetSender, Cyclops cyclops)
        {
            this.packetSender = packetSender;
            this.cyclops = cyclops;
        }

        public override void Process(CyclopsSonarPing sonarPacket)
        {
            cyclops.SonarPing(sonarPacket.Guid);
        }
    }
}
