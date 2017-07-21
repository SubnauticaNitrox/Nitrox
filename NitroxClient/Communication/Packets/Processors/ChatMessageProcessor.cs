using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.Logger;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class ChatMessageProcessor : ClientPacketProcessor<ChatMessage>
    {
        public override void Process(ChatMessage message)
        {
            ClientLogger.IngameMessage(message.PlayerId + ": " + message.Text);
        }
    }
}
