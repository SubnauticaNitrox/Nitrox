using Nitrox.Model.DataStructures;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class MutePlayerProcessor : ClientPacketProcessor<MutePlayer>
{
    private readonly PlayerManager playerManager;

    public delegate void PlayerMuted(ushort playerId, bool muted);
    public PlayerMuted OnPlayerMuted;

    public MutePlayerProcessor(PlayerManager playerManager)
    {
        this.playerManager = playerManager;
    }

    public override void Process(MutePlayer packet)
    {
        // We only need to notice if that's another player than local player
        Optional<RemotePlayer> player = playerManager.Find(packet.SessionId);
        if (player.HasValue)
        {
            player.Value.PlayerContext.IsMuted = packet.Muted;
        }
        OnPlayerMuted(packet.SessionId, packet.Muted);
    }
}
