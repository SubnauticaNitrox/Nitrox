using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class AddEscapePodProcessor : ClientPacketProcessor<AddEscapePod>
    {
        private IMultiplayerSession multiplayerSession;
        private EscapePodManager escapePodManager;

        public AddEscapePodProcessor(IMultiplayerSession multiplayerSession, EscapePodManager escapePodManager)
        {
            this.multiplayerSession = multiplayerSession;
            this.escapePodManager = escapePodManager;
        }

        public override void Process(AddEscapePod packet)
        {
            escapePodManager.AddNewEscapePod(packet.EscapePod);
        }
    }
}
