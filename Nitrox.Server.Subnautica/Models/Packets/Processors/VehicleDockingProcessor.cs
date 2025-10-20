using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

sealed class VehicleDockingProcessor : AuthenticatedPacketProcessor<VehicleDocking>
{
    private readonly PlayerManager playerManager;
    private readonly EntityRegistry entityRegistry;
    private readonly ILogger<VehicleDockingProcessor> logger;

    public VehicleDockingProcessor(PlayerManager playerManager, EntityRegistry entityRegistry, ILogger<VehicleDockingProcessor> logger)
    {
        this.playerManager = playerManager;
        this.entityRegistry = entityRegistry;
        this.logger = logger;
    }

    public override void Process(VehicleDocking packet, Player player)
    {
        if (!entityRegistry.TryGetEntityById(packet.VehicleId, out Entity vehicleEntity))
        {
            logger.ZLogError($"Unable to find vehicle to dock {packet.VehicleId}");
            return;
        }

        if (!entityRegistry.TryGetEntityById(packet.DockId, out Entity dockEntity))
        {
            logger.ZLogError($"Unable to find dock {packet.DockId} for docking vehicle {packet.VehicleId}");
            return;
        }

        entityRegistry.ReparentEntity(vehicleEntity, dockEntity);

        playerManager.SendPacketToOtherPlayers(packet, player);
    }
}
