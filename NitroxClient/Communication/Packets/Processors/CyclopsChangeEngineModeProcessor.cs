using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel_Subnautica.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsChangeEngineModeProcessor : ClientPacketProcessor<CyclopsChangeEngineMode>
    {
        private readonly Cyclops cyclops;

        public CyclopsChangeEngineModeProcessor(Cyclops cyclops)
        {
            this.cyclops = cyclops;
        }

        public override void Process(CyclopsChangeEngineMode motorPacket)
        {
            cyclops.ChangeEngineMode(motorPacket.Id, motorPacket.Mode);
        }
    }
}
