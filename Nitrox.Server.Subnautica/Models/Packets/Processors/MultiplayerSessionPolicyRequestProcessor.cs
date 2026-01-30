using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class MultiplayerSessionPolicyRequestProcessor(IOptions<SubnauticaServerOptions> configProvider, ILogger<MultiplayerSessionPolicyRequestProcessor> logger)
    : IAnonPacketProcessor<MultiplayerSessionPolicyRequest>
{
    private readonly IOptions<SubnauticaServerOptions> configProvider = configProvider;
    private readonly ILogger<MultiplayerSessionPolicyRequestProcessor> logger = logger;

    // This will extend in the future when we look into different options for auth
    public async Task Process(AnonProcessorContext context, MultiplayerSessionPolicyRequest packet)
    {
        logger.ZLogInformation($"Providing session policies...");
        SubnauticaServerOptions options = configProvider.Value;
        await context.ReplyAsync(new MultiplayerSessionPolicy(packet.CorrelationId, options.DisableConsole, options.MaxConnections, options.IsPasswordRequired()));
    }
}
