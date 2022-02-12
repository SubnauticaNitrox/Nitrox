using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.Serialization;

namespace NitroxServer.Communication.Packets.Processors
{
    public class MultiplayerSessionPolicyRequestProcessor : UnauthenticatedPacketProcessor<MultiplayerSessionPolicyRequest>
    {
        private readonly ServerConfig config;

        public MultiplayerSessionPolicyRequestProcessor(ServerConfig config)
        {
            this.config = config;
        }

        // This will extend in the future when we look into different options for auth
        public override void Process(MultiplayerSessionPolicyRequest packet, NitroxConnection connection)
        {
            Log.Info("Providing session policies...");
            connection.SendPacket(new MultiplayerSessionPolicy(packet.CorrelationId, config.DisableConsole, config.MaxConnections, config.IsPasswordRequired));
        }
    }
}
