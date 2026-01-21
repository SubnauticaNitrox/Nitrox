using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class ChatMessageProcessor(ILogger<ChatMessageProcessor> logger) : IAuthPacketProcessor<ChatMessage>
{
    private readonly ILogger<ChatMessageProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, ChatMessage packet)
    {
        if (context.Sender.PlayerContext.IsMuted)
        {
            await context.ReplyAsync(new ChatMessage(ChatMessage.SERVER_ID, "You're currently muted"));
            return;
        }
        logger.ZLogInformation($"<{context.Sender.Name}>: {packet.Text}");
        await context.SendToAllAsync(packet);
    }
}
