using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    class CyclopsDecoyLaunchProcessor : IClientPacketProcessor<CyclopsDecoyLaunch>
    {
        private readonly IPacketSender packetSender;
        private readonly Cyclops cyclops;

        public CyclopsDecoyLaunchProcessor(IPacketSender packetSender, Cyclops cyclops)
        {
            this.packetSender = packetSender;
            this.cyclops = cyclops;
        }

        public Task Process(IPacketProcessContext context, CyclopsDecoyLaunch decoyLaunchPacket)
        {
            cyclops.LaunchDecoy(decoyLaunchPacket.Id);
            return Task.CompletedTask;
        }
    }
}
