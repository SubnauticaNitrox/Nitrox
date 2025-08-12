using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class VehicleOnPilotModeChangedProcessor : ClientPacketProcessor<VehicleOnPilotModeChanged>
{
    private readonly Vehicles vehicles;
    private readonly PlayerManager playerManager;

    public VehicleOnPilotModeChangedProcessor(Vehicles vehicles, PlayerManager playerManager)
    {
        this.vehicles = vehicles;
        this.playerManager = playerManager;
    }

    public override void Process(VehicleOnPilotModeChanged packet)
    {
        if (NitroxEntity.TryGetObjectFrom(packet.VehicleId, out GameObject gameObject))
        {
            // If the vehicle is docked, then we will manually set the piloting mode
            // once the animations complete.  This prevents weird behaviour such as the
            // player existing the vehicle while it is about to dock (the event fires 
            // before the animation completes on the remote player.)
            if (gameObject.TryGetComponent(out Vehicle vehicle) && vehicle.docked)
            {
                return;
            }

            vehicles.SetOnPilotMode(gameObject, packet.PlayerId, packet.IsPiloting);
        }
    }
}
