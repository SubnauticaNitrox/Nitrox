using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours.Gui.InGame;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class UserKickedProcessor : ClientPacketProcessor<PlayerKicked>
    {
        private readonly IMultiplayerSession session;

        public UserKickedProcessor(IMultiplayerSession session)
        {
            this.session = session;
        }

        public override void Process(PlayerKicked packet)
        {
            session.Disconnect();
            PlayerKickedModal.Instance.Show(packet.Reason);
        }
    }
}
