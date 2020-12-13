using Nitrox.Model.Logger;
using Nitrox.Model.Packets;
using Nitrox.Server.Communication.NetworkingLayer;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.Serialization;

namespace Nitrox.Server.Communication.Packets.Processors
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
