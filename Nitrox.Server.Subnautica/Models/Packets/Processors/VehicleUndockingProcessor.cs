using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class VehicleUndockingProcessor : AuthenticatedPacketProcessor<VehicleUndocking>
{
    private readonly PlayerManager playerManager;
    private readonly EntityRegistry entityRegistry;
    private readonly ILogger<VehicleUndockingProcessor> logger;

    public VehicleUndockingProcessor(PlayerManager playerManager, EntityRegistry entityRegistry, ILogger<VehicleUndockingProcessor> logger)
    {
        this.playerManager = playerManager;
        this.entityRegistry = entityRegistry;
        this.logger = logger;
    }

    public override void Process(VehicleUndocking packet, Player player)
    {
        if (packet.UndockingStart)
        {
            if (!entityRegistry.TryGetEntityById(packet.VehicleId, out Entity vehicleEntity))
            {
                logger.ZLogError($"Unable to find vehicle to undock {packet.VehicleId}");
                return;
            }

            if (!entityRegistry.GetEntityById(vehicleEntity.ParentId).HasValue)
            {
                logger.ZLogError($"Unable to find docked vehicles parent {vehicleEntity.ParentId} to undock from");
                return;
            }

            entityRegistry.RemoveFromParent(vehicleEntity);
        }

        playerManager.SendPacketToOtherPlayers(packet, player);
    }
}
