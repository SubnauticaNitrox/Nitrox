using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class VehicleUndockingProcessor(EntityRegistry entityRegistry, ILogger<VehicleUndockingProcessor> logger) : IAuthPacketProcessor<VehicleUndocking>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly ILogger<VehicleUndockingProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, VehicleUndocking packet)
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

        await context.SendToOthersAsync(packet);
    }
}
