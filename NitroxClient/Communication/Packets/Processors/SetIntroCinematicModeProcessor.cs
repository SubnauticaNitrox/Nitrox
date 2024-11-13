using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class SetIntroCinematicModeProcessor : ClientPacketProcessor<SetIntroCinematicMode>
{
    private readonly PlayerManager playerManager;
    private readonly PlayerCinematics playerCinematics;
    private readonly LocalPlayer localPlayer;

    public SetIntroCinematicModeProcessor(PlayerManager playerManager, PlayerCinematics playerCinematics, LocalPlayer localPlayer)
    {
        this.playerManager = playerManager;
        this.playerCinematics = playerCinematics;
        this.localPlayer = localPlayer;
    }

    public override void Process(SetIntroCinematicMode packet)
    {
        if (localPlayer.PlayerId == packet.PlayerId)
        {
            if (packet.PartnerId.HasValue)
            {
                playerCinematics.IntroCinematicPartnerId = packet.PartnerId;
            }

            localPlayer.IntroCinematicMode = packet.Mode;
            return;
        }

        if (playerManager.TryFind(packet.PlayerId, out RemotePlayer remotePlayer))
        {
            remotePlayer.PlayerContext.IntroCinematicMode = packet.Mode;
            return;
        }

        Log.Debug($"SetIntroCinematicMode couldn't find Player with id {packet.PlayerId}. This is normal if player has not yet officially joined.");
    }
}
