using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors;

public class VehicleMovementsPacketProcessor : AuthenticatedPacketProcessor<VehicleMovements>
{
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
                packet.Data.RemoveAt(i);
                Log.WarnOnce($"Player {player.Name} tried updating {movementData.Id}'s position but they don't have the lock on it");
                continue;
            }

            if (entityRegistry.TryGetEntityById(movementData.Id, out WorldEntity worldEntity))
            {
                worldEntity.Transform.Position = movementData.Position;
                worldEntity.Transform.Rotation = movementData.Rotation;

                if (movementData is DrivenVehicleMovementData)
                {
                    // Cyclops' driving wheel is not at relative 0,0,0
                    if (worldEntity.TechType.Name.Equals("Cyclops"))
                    {
                        player.Entity.Transform.LocalPosition = NitroxVector3.Zero; // TODO: set the right position offset in the cyclops
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
