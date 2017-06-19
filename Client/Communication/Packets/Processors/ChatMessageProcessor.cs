using NitroxClient.Communication.Packets.Processors.Base;
using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxClient.Communication.Packets.Processors
{
    public class ChatMessageProcessor : GenericPacketProcessor<ChatMessage>
    {
        public override void Process(ChatMessage message)
        {
            ErrorMessage.AddMessage(message.PlayerId + ": " + message.Text);
        }
    }
}
