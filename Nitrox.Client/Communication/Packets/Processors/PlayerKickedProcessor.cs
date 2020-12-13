using Nitrox.Client.Communication.Abstract;
using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.MonoBehaviours.Gui.InGame;
using Nitrox.Model.Packets;

namespace Nitrox.Client.Communication.Packets.Processors
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
