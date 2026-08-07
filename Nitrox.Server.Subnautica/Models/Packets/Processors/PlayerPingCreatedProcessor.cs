using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlayerPingCreatedProcessor(ILogger<PlayerPingCreatedProcessor> logger) : IAuthPacketProcessor<PlayerPingCreated>
{
    private readonly ILogger<PlayerPingCreatedProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, PlayerPingCreated packet)
    {
        if (packet.VoiceLineIndex >= PlayerPingCreated.VOICE_LINE_COUNT)
        {
            logger.ZLogWarning($"Player {context.Sender.Name} tried to use invalid ping voice line index {packet.VoiceLineIndex}");
            return;
        }

        if (packet.SessionId != context.Sender.SessionId)
        {
            logger.ZLogWarning($"Player {context.Sender.Name} sent a ping with mismatched session id {packet.SessionId}");
        }

        PlayerPingCreated canonicalPacket = new(context.Sender.SessionId, packet.Text, packet.Position, packet.PingId, packet.VoiceLineIndex);
        await context.SendToOthersAsync(canonicalPacket);
    }
}
