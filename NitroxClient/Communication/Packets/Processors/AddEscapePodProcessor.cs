using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class AddEscapePodProcessor : ClientPacketProcessor<AddEscapePod>
    {
        private readonly EscapePodManager escapePodManager;

        public AddEscapePodProcessor(EscapePodManager escapePodManager)
        {
            this.escapePodManager = escapePodManager;
        }

        public override void Process(AddEscapePod packet)
        {
            escapePodManager.AddNewEscapePod(packet.EscapePod);
        }
    }
}
