using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.CinematicController;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Networking.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class PlayerCinematicControllerCallProcessor : IClientPacketProcessor<PlayerCinematicControllerCall>
{
    private readonly PlayerManager playerManager;

    public PlayerCinematicControllerCallProcessor(PlayerManager playerManager)
    {
        this.playerManager = playerManager;
    }

    public Task Process(IPacketProcessContext context, PlayerCinematicControllerCall packet)
    {
        if (!NitroxEntity.TryGetObjectFrom(packet.ControllerID, out GameObject entity))
        {
            return Task.CompletedTask; // Entity can be not spawned yet bc async.
        }

        if (!entity.TryGetComponent(out MultiplayerCinematicReference reference))
        {
            Log.Warn($"Couldn't find {nameof(MultiplayerCinematicReference)} on {entity.name}:{packet.ControllerID}");
            return Task.CompletedTask;
        }

        Optional<RemotePlayer> opPlayer = playerManager.Find(packet.PlayerId);
        Validate.IsPresent(opPlayer);

        if (packet.StartPlaying)
        {
            reference.CallStartCinematicMode(packet.Key, packet.ControllerNameHash, opPlayer.Value);
        }
        else
        {
            reference.CallCinematicModeEnd(packet.Key, packet.ControllerNameHash, opPlayer.Value);
        }

        return Task.CompletedTask;
    }
}
