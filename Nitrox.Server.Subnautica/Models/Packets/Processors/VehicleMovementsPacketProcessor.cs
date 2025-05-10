using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class VehicleMovementsPacketProcessor(EntityRegistry entityRegistry, SimulationOwnershipData simulationOwnershipData, ILogger<VehicleMovementsPacketProcessor> logger) : IAuthPacketProcessor<VehicleMovements>
{
    private static readonly NitroxVector3 cyclopsSteeringWheelRelativePosition = new(-0.05f, 0.97f, -23.54f);

    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly SimulationOwnershipData simulationOwnershipData = simulationOwnershipData;

    public async Task Process(AuthProcessorContext context, VehicleMovements packet)
    {
        for (int i = packet.Data.Count - 1; i >= 0; i--)
        {
            MovementData movementData = packet.Data[i];
            if (simulationOwnershipData.GetPlayerForLock(movementData.Id) != context.Sender.PlayerId)
            {
                // TODO: Warn once
                logger.LogWarning("Player {PlayerId} tried updating {Id}'s position but they don't have the lock on it", context.Sender, movementData.Id);
                // TODO: In the future, add "packet.Data.RemoveAt(i);" and "continue;" to prevent those abnormal situations
            }

            if (entityRegistry.TryGetEntityById(movementData.Id, out WorldEntity worldEntity))
            {
                worldEntity.Transform.Position = movementData.Position;
                worldEntity.Transform.Rotation = movementData.Rotation;

                // TODO: USE DATABASE
                // if (movementData is DrivenVehicleMovementData)
                // {
                //     // Cyclops' driving wheel is at a known position so we need to adapt the position of the player accordingly
                //     if (worldEntity.TechType.Name.Equals("Cyclops"))
                //     {
                //         player.Entity.Transform.LocalPosition = cyclopsSteeringWheelRelativePosition;
                //         player.Position = player.Entity.Transform.Position;
                //     }
                //     else
                //     {
                //         player.Position = movementData.Position;
                //         player.Rotation = movementData.Rotation;
                //     }
                // }
            }
        }

        if (packet.Data.Count > 0)
        {
            await context.ReplyToOthers(packet);
        }
    }
}
