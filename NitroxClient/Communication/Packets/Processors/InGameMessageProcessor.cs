using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Logger;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    class InGameMessageProcessor : ClientPacketProcessor<InGameMessageEvent>
    {
        public override void Process(InGameMessageEvent packet)
        {
            Log.InGame(packet.Message);
        }
    }
}
