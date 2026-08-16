using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.CinematicController;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class PlayerCinematicControllerCallProcessor(PlayerManager playerManager, LocalPlayer localPlayer) : IClientPacketProcessor<PlayerCinematicControllerCall>
{
    private readonly LocalPlayer localPlayer = localPlayer;
    private readonly PlayerManager playerManager = playerManager;

    public Task Process(ClientProcessorContext context, PlayerCinematicControllerCall packet)
    {
        if (!NitroxEntity.TryGetObjectFrom(packet.ControllerID, out GameObject entity))
        {
            return Task.CompletedTask;
        }

        if (!entity.TryGetComponent(out MultiplayerCinematicReference reference))
        {
            Log.Warn($"Couldn't find {nameof(MultiplayerCinematicReference)} on {entity.name}:{packet.ControllerID}");
            return Task.CompletedTask;
        }

        Optional<RemotePlayer> opPlayer = playerManager.Find(packet.SessionId);
        Validate.IsPresent(opPlayer);

        if (packet.StartPlaying)
        {
            reference.CallStartCinematicMode(packet.Key, packet.ControllerNameHash, opPlayer.Value, localPlayer);
        }
        else
        {
            reference.CallCinematicModeEnd(packet.Key, packet.ControllerNameHash, opPlayer.Value, localPlayer);
        }
        return Task.CompletedTask;
    }
}
