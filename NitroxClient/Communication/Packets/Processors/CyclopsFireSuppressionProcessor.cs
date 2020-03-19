using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel_Subnautica.Packets;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsFireSuppressionProcessor : ClientPacketProcessor<CyclopsFireSuppression>
    {
        private readonly IPacketSender packetSender;
        private readonly Cyclops cyclops;

        public CyclopsFireSuppressionProcessor(IPacketSender packetSender, Cyclops cyclops)
        {
            this.packetSender = packetSender;
            this.cyclops = cyclops;
        }

        public override void Process(CyclopsFireSuppression fireSuppressionPacket)
        {
            cyclops.StartFireSuppression(fireSuppressionPacket.Id);
        }
    }
}
