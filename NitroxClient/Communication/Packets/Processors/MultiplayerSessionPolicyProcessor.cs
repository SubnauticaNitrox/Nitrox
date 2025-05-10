using NitroxClient.Communication.Abstract;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class MultiplayerSessionPolicyProcessor : IClientPacketProcessor<MultiplayerSessionPolicy>
    {
        private readonly IMultiplayerSession multiplayerSession;

        public MultiplayerSessionPolicyProcessor(IMultiplayerSession multiplayerSession)
        {
            this.multiplayerSession = multiplayerSession;
        }

        public Task Process(IPacketProcessContext context, MultiplayerSessionPolicy packet)
        {
            Log.Info("Processing session policy information.");
            multiplayerSession.ProcessSessionPolicy(packet);

            return Task.CompletedTask;
        }
    }
}
