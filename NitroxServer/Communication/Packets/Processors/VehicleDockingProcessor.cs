using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;
using static NitroxModel.DisplayStatusCodes;
namespace NitroxServer.Communication.Packets.Processors;

public class VehicleDockingProcessor : AuthenticatedPacketProcessor<VehicleDocking>
{
    private readonly PlayerManager playerManager;
    private readonly EntityRegistry entityRegistry;

    public VehicleDockingProcessor(PlayerManager playerManager, EntityRegistry entityRegistry)
    {
        this.playerManager = playerManager;
        this.entityRegistry = entityRegistry;
    }

    public override void Process(VehicleDocking packet, Player player)
    {
        if (!entityRegistry.TryGetEntityById(packet.VehicleId, out Entity vehicleEntity))
        {
            PrintStatusCode(StatusCode.INVALID_PACKET, $"Unable to find vehicle to dock {packet.VehicleId}");
            return;
        }

        if (!entityRegistry.TryGetEntityById(packet.DockId, out Entity dockEntity))
        {
            PrintStatusCode(StatusCode.INVALID_PACKET, $"Unable to find dock {packet.DockId} for docking vehicle {packet.VehicleId}");
            return;
        }

        entityRegistry.ReparentEntity(vehicleEntity, dockEntity);

        playerManager.SendPacketToOtherPlayers(packet, player);
    }
}
