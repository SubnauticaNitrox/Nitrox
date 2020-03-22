using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel_Subnautica.Packets;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsChangeSonarModeProcessor : ClientPacketProcessor<CyclopsChangeSonarMode>
    {
        private readonly IPacketSender packetSender;
        private readonly Cyclops cyclops;

        public CyclopsChangeSonarModeProcessor(IPacketSender packetSender, Cyclops cyclops)
        {
            this.packetSender = packetSender;
            this.cyclops = cyclops;
        }

        public override void Process(CyclopsChangeSonarMode sonarPacket)
        {
            cyclops.ChangeSonarMode(sonarPacket.Id, sonarPacket.IsOn);
        }
    }
}
