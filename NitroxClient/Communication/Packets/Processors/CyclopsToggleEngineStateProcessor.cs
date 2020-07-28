using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel_Subnautica.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsToggleEngineStateProcessor : ClientPacketProcessor<CyclopsToggleEngineState>
    {
        private readonly Cyclops cyclops;

        public CyclopsToggleEngineStateProcessor(Cyclops cyclops)
        {
            this.cyclops = cyclops;
        }

        public override void Process(CyclopsToggleEngineState packet)
        {
            cyclops.ToggleEngineState(packet.Id, packet.IsStarting, packet.IsOn);
        }
    }
}
