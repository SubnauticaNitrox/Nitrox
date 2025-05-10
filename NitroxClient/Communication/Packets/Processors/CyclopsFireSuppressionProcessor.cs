using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsFireSuppressionProcessor : IClientPacketProcessor<CyclopsFireSuppression>
    {
        private readonly IPacketSender packetSender;
        private readonly Cyclops cyclops;

        public CyclopsFireSuppressionProcessor(IPacketSender packetSender, Cyclops cyclops)
        {
            this.packetSender = packetSender;
            this.cyclops = cyclops;
        }

        public Task Process(IPacketProcessContext context, CyclopsFireSuppression fireSuppressionPacket)
        {
            cyclops.StartFireSuppression(fireSuppressionPacket.Id);
            return Task.CompletedTask;
        }
    }
}
