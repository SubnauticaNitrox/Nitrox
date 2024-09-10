using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class SetIntroCinematicModeProcessor : ClientPacketProcessor<SetIntroCinematicMode>
{
    private readonly PlayerManager playerManager;
    private readonly LocalPlayer localPlayer;

    public SetIntroCinematicModeProcessor(PlayerManager playerManager, LocalPlayer localPlayer)
    {
        this.playerManager = playerManager;
        this.localPlayer = localPlayer;
    }

    public override void Process(SetIntroCinematicMode packet)
    {
        if (!packet.PlayerId.HasValue)
        {
            Log.Error("playerId of SetIntroCinematicMode packet is null which is not expected.");
            return;
        }

        if (localPlayer.PlayerId == packet.PlayerId)
        {
            localPlayer.IntroCinematicMode = packet.Mode;
            return;
        }

        if (playerManager.TryFind(packet.PlayerId.Value, out RemotePlayer remotePlayer))
        {
            remotePlayer.PlayerContext.IntroCinematicMode = packet.Mode;
            return;
        }

        Log.Debug($"SetIntroCinematicMode couldn't find Player with id {packet.PlayerId}. This is normal if player has not yet officially joined.");
    }
}
