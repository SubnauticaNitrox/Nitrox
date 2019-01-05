using NitroxClient.Communication.Abstract;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic
{
    // TODO: Refactor to some player API instead of this tiny class.
    public class Chat
    {
        private readonly IMultiplayerSession multiplayerSession;

        public Chat(IMultiplayerSession multiplayerSession)
        {
            this.multiplayerSession = multiplayerSession;
        }

        public void SendChatMessage(string text)
        {
            ChatMessage message = new ChatMessage(multiplayerSession.Reservation.PlayerId, text);
            multiplayerSession.Send(message);
        }
    }
}
