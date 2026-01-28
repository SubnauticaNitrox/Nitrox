using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class VehicleOnPilotModeChangedProcessor(Vehicles vehicles, PlayerManager playerManager) : IClientPacketProcessor<VehicleOnPilotModeChanged>
{
    private readonly PlayerManager playerManager = playerManager;
    private readonly Vehicles vehicles = vehicles;

    public Task Process(ClientProcessorContext context, VehicleOnPilotModeChanged packet)
    {
        if (NitroxEntity.TryGetObjectFrom(packet.VehicleId, out GameObject gameObject))
        {
            // If the vehicle is docked, then we will manually set the piloting mode
            // once the animations complete.  This prevents weird behaviour such as the
            // player existing the vehicle while it is about to dock (the event fires 
            // before the animation completes on the remote player.)
            if (gameObject.TryGetComponent(out Vehicle vehicle) && vehicle.docked)
            {
                return Task.CompletedTask;
            }

            vehicles.SetOnPilotMode(gameObject, packet.SessionId, packet.IsPiloting);
        }
        return Task.CompletedTask;
    }
}
