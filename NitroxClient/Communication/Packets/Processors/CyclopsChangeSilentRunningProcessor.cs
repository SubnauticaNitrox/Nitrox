using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel_Subnautica.Packets;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors
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
