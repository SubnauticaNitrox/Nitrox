using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Vehicles;

namespace NitroxServer.Communication.Packets.Processors;

public class VehicleDestroyedPacketProcessor : AuthenticatedPacketProcessor<VehicleDestroyed>
{
    private readonly PlayerManager playerManager;
    private readonly VehicleManager vehicleManager;

    public VehicleDestroyedPacketProcessor(PlayerManager playerManager, VehicleManager vehicleManager)
    {
        this.playerManager = playerManager;
        this.vehicleManager = vehicleManager;
    }

    public override void Process(VehicleDestroyed packet, Player player)
    {
        vehicleManager.RemoveVehicle(packet.Id);
        playerManager.SendPacketToOtherPlayers(packet, player);
    }
}
