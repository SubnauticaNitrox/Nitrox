using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class VehicleMovementsPacketProcessor(EntityRegistry entityRegistry, SimulationOwnershipData simulationOwnershipData, ILogger<VehicleMovementsPacketProcessor> logger)
    : IAuthPacketProcessor<VehicleMovements>
{
    private static readonly NitroxVector3 CyclopsSteeringWheelRelativePosition = new(-0.05f, 0.97f, -23.54f);

    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly SimulationOwnershipData simulationOwnershipData = simulationOwnershipData;
    private readonly ILogger<VehicleMovementsPacketProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, VehicleMovements packet)
    {
        for (int i = packet.Data.Count - 1; i >= 0; i--)
        {
            MovementData movementData = packet.Data[i];
            if (simulationOwnershipData.GetPlayerForLock(movementData.Id) != context.Sender)
            {
                logger.ZLogErrorOnce($"Player {context.Sender.Name} tried updating {movementData.Id}'s position but they don't have the lock on it");
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
                        context.Sender.Entity.Transform.LocalPosition = CyclopsSteeringWheelRelativePosition;
                        context.Sender.Position = context.Sender.Entity.Transform.Position;
                    }
                    else
                    {
                        context.Sender.Position = movementData.Position;
                        context.Sender.Rotation = movementData.Rotation;
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
