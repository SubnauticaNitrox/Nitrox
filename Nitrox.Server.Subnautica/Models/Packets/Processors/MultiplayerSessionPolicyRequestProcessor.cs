using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Configuration;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class MultiplayerSessionPolicyRequestProcessor(IOptions<SubnauticaServerOptions> optionsProvider, ILogger<MultiplayerSessionPolicyRequestProcessor> logger) : IAnonPacketProcessor<MultiplayerSessionPolicyRequest>
{
    private readonly IOptions<SubnauticaServerOptions> optionsProvider = optionsProvider;
    private readonly ILogger<MultiplayerSessionPolicyRequestProcessor> logger = logger;

    // This will extend in the future when we look into different options for auth
    public async Task Process(AnonProcessorContext context, MultiplayerSessionPolicyRequest packet)
    {
        logger.ZLogInformation($"Providing session policies...");
        SubnauticaServerOptions options = optionsProvider.Value;
        await context.ReplyToSender(new MultiplayerSessionPolicy(packet.CorrelationId, options.DisableConsole, options.MaxConnections, options.IsPasswordRequired()));
    }
}
