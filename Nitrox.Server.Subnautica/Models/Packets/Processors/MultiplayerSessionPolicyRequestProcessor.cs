using Nitrox.Model.Packets;
using Nitrox.Model.Serialization;
using Nitrox.Server.Subnautica.Models.Communication;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors
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
