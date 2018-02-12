using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    class ChatMessageProcessor : ClientPacketProcessor<ChatMessage>
    {
        public ChatMessageProcessor()
        {
        }

        public override void Process(ChatMessage message)
        {
            if (Chat.Main != null)
            {
                Chat.Main.WriteLocalMessage(message);
            }
        }
    }
}
