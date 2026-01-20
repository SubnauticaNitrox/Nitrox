using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class ChatMessageProcessor : AuthenticatedPacketProcessor<ChatMessage>
{
    private readonly IPacketSender packetSender;
    private readonly ILogger<ChatMessageProcessor> logger;

    public ChatMessageProcessor(IPacketSender packetSender, ILogger<ChatMessageProcessor> logger)
    {
        this.packetSender = packetSender;
        this.logger = logger;
    }

    public override void Process(ChatMessage packet, Player player)
    {
        if (player.PlayerContext.IsMuted)
        {
            packetSender.SendPacketAsync(new ChatMessage(ChatMessage.SERVER_ID, "You're currently muted"), player.SessionId);
            return;
        }
        logger.ZLogInformation($"<{player.Name}>: {packet.Text}");
        packetSender.SendPacketToAllAsync(packet);
    }
}
