using NitroxModel_Subnautica.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Vehicles;

namespace NitroxServer_Subnautica.Communication.Packets.Processors;

public class CyclopsDestroyedProcessor : AuthenticatedPacketProcessor<CyclopsDestroyed>
{
    private readonly PlayerManager playerManager;
    private readonly VehicleManager vehicleManager;

    public CyclopsDestroyedProcessor(PlayerManager playerManager, VehicleManager vehicleManager)
    {
        this.playerManager = playerManager;
        this.vehicleManager = vehicleManager;
    }

    public override void Process(CyclopsDestroyed packet, NitroxServer.Player simulatingPlayer)
    {
        vehicleManager.RemoveVehicle(packet.Id);
        playerManager.SendPacketToOtherPlayers(packet, simulatingPlayer);
    }
}
