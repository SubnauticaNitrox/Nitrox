using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.ChatUI;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    class ChatMessageProcessor : ClientPacketProcessor<ChatMessage>
    {
        private readonly PlayerChatManager chatManager;

        public ChatMessageProcessor(PlayerChatManager chatManager)
        {
            this.chatManager = chatManager;
        }

        public override void Process(ChatMessage message)
        {
            chatManager.WriteMessage(message.PlayerId + ": " + message.Text);
        }
    }
}
