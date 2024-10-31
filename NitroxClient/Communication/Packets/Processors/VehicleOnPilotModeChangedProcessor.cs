using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class VehicleOnPilotModeChangedProcessor : ClientPacketProcessor<VehicleOnPilotModeChanged>
{
    private readonly IPacketSender packetSender;
    private readonly Vehicles vehicles;
    private readonly PlayerManager playerManager;

    public VehicleOnPilotModeChangedProcessor(IPacketSender packetSender, Vehicles vehicles, PlayerManager playerManager)
    {
        this.packetSender = packetSender;
        this.vehicles = vehicles;
        this.playerManager = playerManager;
    }

    public override void Process(VehicleOnPilotModeChanged packet)
    {
        if (NitroxEntity.TryGetComponentFrom(packet.VehicleId, out Vehicle vehicle))
        {
            // If the vehicle is docked, then we will manually set the piloting mode
            // once the animations complete.  This prevents weird behaviour such as the
            // player existing the vehicle while it is about to dock (the event fires 
            // before the animation completes on the remote player.)
            if (!vehicle.docked)
            {
                vehicles.SetOnPilotMode(vehicle.gameObject, packet.PlayerId, packet.IsPiloting);
            }
        }
    }
}
