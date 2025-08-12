using NitroxModel.Packets;
using NitroxModel.Serialization;
using NitroxServer.Communication.Packets.Processors.Abstract;

namespace NitroxServer.Communication.Packets.Processors
{
    public class MultiplayerSessionPolicyRequestProcessor : UnauthenticatedPacketProcessor<MultiplayerSessionPolicyRequest>
    {
        private readonly SubnauticaServerConfig config;

        public MultiplayerSessionPolicyRequestProcessor(SubnauticaServerConfig config)
        {
            this.config = config;
        }

        // This will extend in the future when we look into different options for auth
        public override void Process(MultiplayerSessionPolicyRequest packet, INitroxConnection connection)
        {
            Log.Info("Providing session policies...");
            connection.SendPacket(new MultiplayerSessionPolicy(packet.CorrelationId, config.DisableConsole, config.MaxConnections, config.IsPasswordRequired()));
        }
    }
}
