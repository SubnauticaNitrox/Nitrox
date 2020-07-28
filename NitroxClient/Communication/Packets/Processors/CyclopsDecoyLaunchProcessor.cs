using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel_Subnautica.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    class CyclopsDecoyLaunchProcessor : ClientPacketProcessor<CyclopsDecoyLaunch>
    {
        private readonly Cyclops cyclops;

        public CyclopsDecoyLaunchProcessor(Cyclops cyclops)
        {
            this.cyclops = cyclops;
        }

        public override void Process(CyclopsDecoyLaunch decoyLaunchPacket)
        {
            cyclops.LaunchDecoy(decoyLaunchPacket.Id);
        }
    }
}
