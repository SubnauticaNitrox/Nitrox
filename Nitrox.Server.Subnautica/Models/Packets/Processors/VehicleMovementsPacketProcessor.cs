using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Connected;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class VehicleMovementsPacketProcessor(EntityRegistry entityRegistry, SimulationOwnershipData simulationOwnershipData, PlayerService playerService, ILogger<VehicleMovementsPacketProcessor> logger)
    : IAuthPacketProcessor<VehicleMovements>
{
    private static readonly NitroxVector3 CyclopsSteeringWheelRelativePosition = new(-0.05f, 0.97f, -23.54f);

    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly SimulationOwnershipData simulationOwnershipData = simulationOwnershipData;
    private readonly PlayerService playerService = playerService;
    private readonly ILogger<VehicleMovementsPacketProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, VehicleMovements packet)
    {
        for (int i = packet.Data.Count - 1; i >= 0; i--)
        {
            MovementData movementData = packet.Data[i];
            if (simulationOwnershipData.GetSessionIdForLock(movementData.Id) != context.Sender)
            {
                logger.ZLogErrorOnce($"Player {playerService.GetProperty<NameProperty>(context.Sender).Value} tried updating {movementData.Id}'s position but they don't have the lock on it");
                // TODO: In the future, add "packet.Data.RemoveAt(i);" and "continue;" to prevent those abnormal situations
            }

            if (entityRegistry.TryGetEntityById(movementData.Id, out WorldEntity worldEntity))
            {
                worldEntity.Transform.Position = movementData.Position;
                worldEntity.Transform.Rotation = movementData.Rotation;

                if (movementData is DrivenVehicleMovementData)
                {
                    // Cyclops' driving wheel is at a known position so we need to adapt the position of the player accordingly
                    if (worldEntity.TechType.Name.Equals("Cyclops"))
                    {
                        PlayerEntity senderEntity = playerService.GetProperty<EntityProperty>(context.Sender).Value;
                        senderEntity.Transform.LocalPosition = CyclopsSteeringWheelRelativePosition;
                        playerService.GetProperty<PositionProperty>(context.Sender).Value = senderEntity.Transform.Position;
                    }
                    else
                    {
                        playerService.GetProperty<PositionProperty>(context.Sender).Value = movementData.Position;
                        playerService.GetProperty<RotationProperty>(context.Sender).Value = movementData.Rotation;
                    }
                }
            }
        }

        if (packet.Data.Count > 0)
        {
            await context.SendToOthersAsync(packet);
        }
    }
}
