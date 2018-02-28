using NitroxClient.Communication.Abstract;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic
{
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
