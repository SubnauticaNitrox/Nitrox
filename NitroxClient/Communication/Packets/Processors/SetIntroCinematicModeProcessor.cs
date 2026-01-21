using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.PlayerLogic;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class SetIntroCinematicModeProcessor(PlayerManager playerManager, PlayerCinematics playerCinematics, LocalPlayer localPlayer) : IClientPacketProcessor<SetIntroCinematicMode>
{
    private readonly LocalPlayer localPlayer = localPlayer;
    private readonly PlayerCinematics playerCinematics = playerCinematics;
    private readonly PlayerManager playerManager = playerManager;

    public Task Process(ClientProcessorContext context, SetIntroCinematicMode packet)
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
