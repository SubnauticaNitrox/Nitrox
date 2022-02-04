using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel_Subnautica.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsChangeSonarModeProcessor : ClientPacketProcessor<CyclopsChangeSonarMode>
    {
        private readonly Cyclops cyclops;

        public CyclopsChangeSonarModeProcessor(Cyclops cyclops)
        {
            this.cyclops = cyclops;
        }

        public override void Process(CyclopsChangeSonarMode sonarPacket)
        {
            cyclops.ChangeSonarMode(sonarPacket.Id, sonarPacket.IsOn);
        }
    }
}
