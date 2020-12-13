using Nitrox.Client.Communication.Abstract;
using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.GameLogic;
using Nitrox.Model.Subnautica.Packets;

namespace Nitrox.Client.Communication.Packets.Processors
{
    public class CyclopsToggleEngineStateProcessor : ClientPacketProcessor<CyclopsToggleEngineState>
    {
        private readonly IPacketSender packetSender;
        private readonly Cyclops cyclops;

        public CyclopsToggleEngineStateProcessor(IPacketSender packetSender, Cyclops cyclops)
        {
            this.packetSender = packetSender;
            this.cyclops = cyclops;
        }

        public override void Process(CyclopsToggleEngineState enginePacket)
        {
            cyclops.ToggleEngineState(enginePacket.Id, enginePacket.IsStarting, enginePacket.IsOn);
        }
    }
}
