using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.Logger;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class ChatMessageProcessor : ClientPacketProcessor<ChatMessage>
    {
        public override void Process(ChatMessage message)
        {
            ClientLogger.WriteLine(message.PlayerId + ": " + message.Text);
        }
    }
}
