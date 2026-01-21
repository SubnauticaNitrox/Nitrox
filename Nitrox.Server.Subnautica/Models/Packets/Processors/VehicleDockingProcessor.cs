using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

sealed class VehicleDockingProcessor : IAuthPacketProcessor<VehicleDocking>
{
    private readonly IPacketSender packetSender;
    private readonly EntityRegistry entityRegistry;
    private readonly ILogger<VehicleDockingProcessor> logger;

    public VehicleDockingProcessor(IPacketSender packetSender, EntityRegistry entityRegistry, ILogger<VehicleDockingProcessor> logger)
    {
        this.packetSender = packetSender;
        this.entityRegistry = entityRegistry;
        this.logger = logger;
    }

    public async Task Process(AuthProcessorContext context, VehicleDocking packet)
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

        await context.SendToOthersAsync(packet);
    }
}
