using Nitrox.Client.Communication.Abstract;
using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.GameLogic;
using Nitrox.Model.Packets;

namespace Nitrox.Client.Communication.Packets.Processors
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
