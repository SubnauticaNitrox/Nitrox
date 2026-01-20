using Nitrox.Server.Subnautica.Models.Communication;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class MultiplayerSessionPolicyRequestProcessor : UnauthenticatedPacketProcessor<MultiplayerSessionPolicyRequest>
{
    private readonly IPacketSender packetSender;
    private readonly IOptions<SubnauticaServerOptions> configProvider;
    private readonly ILogger<MultiplayerSessionPolicyRequestProcessor> logger;

    public MultiplayerSessionPolicyRequestProcessor(IPacketSender packetSender, IOptions<SubnauticaServerOptions> configProvider, ILogger<MultiplayerSessionPolicyRequestProcessor> logger)
    {
        this.packetSender = packetSender;
        this.configProvider = configProvider;
        this.logger = logger;
    }

    // This will extend in the future when we look into different options for auth
    public override void Process(MultiplayerSessionPolicyRequest packet, INitroxConnection connection)
    {
        logger.ZLogInformation($"Providing session policies...");
        SubnauticaServerOptions options = configProvider.Value;
        packetSender.SendPacketAsync(new MultiplayerSessionPolicy(packet.CorrelationId, options.DisableConsole, options.MaxConnections, options.IsPasswordRequired()), connection.SessionId);
    }
}
