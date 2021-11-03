using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class JoinQueueDequeuedProcessor : ClientPacketProcessor<JoinQueueDequeued>
    {
        private readonly IMultiplayerSession multiplayerSession;

        public JoinQueueDequeuedProcessor(IMultiplayerSession multiplayerSession)
        {
            this.multiplayerSession = multiplayerSession;
        }

        public override void Process(JoinQueueDequeued packet)
        {
            multiplayerSession.ProcessJoinQueueDequeuedPacket(packet);
        }
    }
}
