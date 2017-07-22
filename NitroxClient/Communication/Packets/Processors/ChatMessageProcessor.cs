using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class ChatMessageProcessor : ClientPacketProcessor<ChatMessage>
    {
        public override void Process(ChatMessage message)
        {
            ErrorMessage.AddMessage(message.PlayerId + ": " + message.Text);
        }
    }
}
