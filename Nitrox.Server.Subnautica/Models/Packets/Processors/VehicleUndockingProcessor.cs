using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class VehicleUndockingProcessor : AuthenticatedPacketProcessor<VehicleUndocking>
{
    private readonly IPacketSender packetSender;
    private readonly EntityRegistry entityRegistry;
    private readonly ILogger<VehicleUndockingProcessor> logger;

    public VehicleUndockingProcessor(IPacketSender packetSender, EntityRegistry entityRegistry, ILogger<VehicleUndockingProcessor> logger)
    {
        this.packetSender = packetSender;
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

        packetSender.SendPacketToOthersAsync(packet, player.SessionId);
    }
}
