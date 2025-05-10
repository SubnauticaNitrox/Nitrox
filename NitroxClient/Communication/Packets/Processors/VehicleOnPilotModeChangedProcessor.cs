using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Networking.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class VehicleOnPilotModeChangedProcessor : IClientPacketProcessor<VehicleOnPilotModeChanged>
{
    private readonly Vehicles vehicles;
    private readonly PlayerManager playerManager;

    public VehicleOnPilotModeChangedProcessor(Vehicles vehicles, PlayerManager playerManager)
    {
        this.vehicles = vehicles;
        this.playerManager = playerManager;
    }

    public Task Process(IPacketProcessContext context, VehicleOnPilotModeChanged packet)
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

            vehicles.SetOnPilotMode(gameObject, packet.PlayerId, packet.IsPiloting);
        }

        return Task.CompletedTask;
    }
}
