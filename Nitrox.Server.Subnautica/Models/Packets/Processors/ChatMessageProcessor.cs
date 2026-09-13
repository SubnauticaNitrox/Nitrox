using Nitrox.Model.Core;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Connected;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class ChatMessageProcessor(PlayerService playerService, ILogger<ChatMessageProcessor> logger) : IAuthPacketProcessor<ChatMessage>
{
    private readonly PlayerService playerService = playerService;
    private readonly ILogger<ChatMessageProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, ChatMessage packet)
    {
        if (playerService.GetProperty<IsMutedProperty>(context.Sender).Value)
        {
            await context.ReplyAsync(new ChatMessage((SessionId)SessionId.SERVER_ID, "You're currently muted"));
            return;
        }
        logger.ZLogInformation($"<{playerService.GetProperty<NameProperty>(context.Sender).Value}>: {packet.Text}");
        await context.SendToAllAsync(packet);
    }
}
