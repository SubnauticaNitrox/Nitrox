using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel_Subnautica.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsChangeSilentRunningProcessor : ClientPacketProcessor<CyclopsChangeSilentRunning>
    {
        private readonly Cyclops cyclops;

        public CyclopsChangeSilentRunningProcessor(Cyclops cyclops)
        {
            this.cyclops = cyclops;
        }

        public override void Process(CyclopsChangeSilentRunning packet)
        {
            cyclops.ChangeSilentRunning(packet.Id, packet.IsOn);
        }
    }
}
