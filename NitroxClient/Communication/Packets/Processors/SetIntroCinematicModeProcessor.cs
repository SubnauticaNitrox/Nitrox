using NitroxClient.GameLogic;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class SetIntroCinematicModeProcessor : IClientPacketProcessor<SetIntroCinematicMode>
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

    public Task Process(IPacketProcessContext context, SetIntroCinematicMode packet)
    {
        if (localPlayer.PlayerId == packet.PlayerId)
        {
            if (packet.PartnerId.HasValue)
            {
                playerCinematics.IntroCinematicPartnerId = packet.PartnerId;
            }

            localPlayer.IntroCinematicMode = packet.Mode;
            return Task.CompletedTask;
        }

        if (playerManager.TryFind(packet.PlayerId, out RemotePlayer remotePlayer))
        {
            remotePlayer.PlayerContext.IntroCinematicMode = packet.Mode;
            return Task.CompletedTask;
        }

        Log.Debug($"SetIntroCinematicMode couldn't find Player with id {packet.PlayerId}. This is normal if player has not yet officially joined.");

        return Task.CompletedTask;
    }
}
