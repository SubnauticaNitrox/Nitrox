using Nitrox.Client.Communication.Abstract;
using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.GameLogic;
using Nitrox.Model.Subnautica.Packets;

namespace Nitrox.Client.Communication.Packets.Processors
{
    public class CyclopsChangeSilentRunningProcessor : ClientPacketProcessor<CyclopsChangeSilentRunning>
    {
        private readonly IPacketSender packetSender;
        private readonly Cyclops cyclops;

        public CyclopsChangeSilentRunningProcessor(IPacketSender packetSender, Cyclops cyclops)
        {
            this.packetSender = packetSender;
            this.cyclops = cyclops;
        }

        public override void Process(CyclopsChangeSilentRunning packet)
        {
            cyclops.ChangeSilentRunning(packet.Id, packet.IsOn);
        }
    }
}
