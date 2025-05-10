using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.Respositories;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class ChatMessageProcessor(PlayerRepository playerRepository, ILogger<ChatMessageProcessor> logger) : IAuthPacketProcessor<ChatMessage>
{
    private readonly PlayerRepository playerRepository = playerRepository;
    private readonly ILogger<ChatMessageProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, ChatMessage packet)
    {
        string playerName = await playerRepository.GetPlayerNameIfNotMuted(context.Sender.PlayerId);
        if (playerName == null)
        {
            await context.ReplyToSender(new ChatMessage(SessionId.SERVER_ID, "You're currently muted"));
            return;
        }
        await context.ReplyToAll(packet);
        logger.ZLogInformation($"{playerName:@PlayerName}: {packet.Text:@ChatMessage}");
    }
}
