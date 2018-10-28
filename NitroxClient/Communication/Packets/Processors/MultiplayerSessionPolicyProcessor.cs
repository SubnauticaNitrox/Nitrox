using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Logger;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class MultiplayerSessionPolicyProcessor : ClientPacketProcessor<MultiplayerSessionPolicy>
    {
        private readonly IMultiplayerSession multiplayerSession;
        private readonly INitroxLogger log;

        public MultiplayerSessionPolicyProcessor(IMultiplayerSession multiplayerSession, INitroxLogger logger)
        {
            this.multiplayerSession = multiplayerSession;
            log = logger;
        }

        public override void Process(MultiplayerSessionPolicy packet)
        {
            log.Info("Processing session policy information.");
            multiplayerSession.ProcessSessionPolicy(packet);
        }
    }
}
