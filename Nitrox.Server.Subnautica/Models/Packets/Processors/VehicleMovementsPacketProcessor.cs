using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class VehicleMovementsPacketProcessor : AuthenticatedPacketProcessor<VehicleMovements>
{
    private static readonly NitroxVector3 CyclopsSteeringWheelRelativePosition = new(-0.05f, 0.97f, -23.54f);

    private readonly PlayerManager playerManager;
    private readonly EntityRegistry entityRegistry;
    private readonly SimulationOwnershipData simulationOwnershipData;

    public VehicleMovementsPacketProcessor(PlayerManager playerManager, EntityRegistry entityRegistry, SimulationOwnershipData simulationOwnershipData)
    {
        this.playerManager = playerManager;
        this.entityRegistry = entityRegistry;
        this.simulationOwnershipData = simulationOwnershipData;
    }

    public override void Process(VehicleMovements packet, Player player)
    {
        for (int i = packet.Data.Count - 1; i >= 0; i--)
        {
            MovementData movementData = packet.Data[i];
            if (simulationOwnershipData.GetPlayerForLock(movementData.Id) != player)
            {
                Log.WarnOnce($"Player {player.Name} tried updating {movementData.Id}'s position but they don't have the lock on it");
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
                        player.Entity.Transform.LocalPosition = CyclopsSteeringWheelRelativePosition;
                        player.Position = player.Entity.Transform.Position;
                    }
                    else
                    {
                        player.Position = movementData.Position;
                        player.Rotation = movementData.Rotation;
                    }
                }
            }
        }

        if (packet.Data.Count > 0)
        {
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
