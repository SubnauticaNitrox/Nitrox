using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours.Gui.Modals;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class UserKickedProcessor : IClientPacketProcessor<PlayerKicked>
    {
        private readonly IMultiplayerSession session;

        public UserKickedProcessor(IMultiplayerSession session)
        {
            this.session = session;
        }

        public Task Process(IPacketProcessContext context, PlayerKicked packet)
        {
            string message = Language.main.Get("Nitrox_PlayerKicked");

            if (!string.IsNullOrEmpty(packet.Reason))
            {
                message += $"\n {packet.Reason}";
            }

            session.Disconnect();
            Modal.Get<KickedModal>()?.Show(message);

            return Task.CompletedTask;
        }
    }
}
