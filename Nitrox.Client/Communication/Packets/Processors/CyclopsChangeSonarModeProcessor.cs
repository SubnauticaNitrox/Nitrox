using Nitrox.Client.Communication.Abstract;
using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.GameLogic;
using Nitrox.Model.Subnautica.Packets;

namespace Nitrox.Client.Communication.Packets.Processors
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
