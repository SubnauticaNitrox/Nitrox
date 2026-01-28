using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class MutePlayerProcessor(PlayerManager playerManager) : IClientPacketProcessor<MutePlayer>
{
    public delegate void PlayerMuted(SessionId sessionId, bool muted);

    private readonly PlayerManager playerManager = playerManager;
    public PlayerMuted OnPlayerMuted;

    public Task Process(ClientProcessorContext context, MutePlayer packet)
    {
        // We only need to notice if that's another player than local player
        Optional<RemotePlayer> player = playerManager.Find(packet.SessionId);
        if (player.HasValue)
        {
            player.Value.PlayerContext.IsMuted = packet.Muted;
        }
        OnPlayerMuted(packet.SessionId, packet.Muted);
        return Task.CompletedTask;
    }
}
