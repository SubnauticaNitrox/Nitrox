using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.ChatUI;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    class ChatMessageProcessor : ClientPacketProcessor<ChatMessage>
    {
        private PlayerChatManager chatManager;

        public ChatMessageProcessor(PlayerChatManager chatManager)
        {
            this.chatManager = chatManager;
        }

        public override void Process(ChatMessage message)
        {
            this.chatManager.WriteMessage(message.PlayerId + ": " + message.Text);
        }
    }
}
