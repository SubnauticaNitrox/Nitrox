using System;
using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.Packets;

namespace NitroxServer.GameLogic.Entities;

public class EntitySimulation
{
    private const SimulationLockType DEFAULT_ENTITY_SIMULATION_LOCKTYPE = SimulationLockType.TRANSIENT;

    private readonly EntityRegistry entityRegistry;
    private readonly WorldEntityManager worldEntityManager;
    private readonly PlayerManager playerManager;
    private readonly ISimulationWhitelist simulationWhitelist;
    private readonly SimulationOwnershipData simulationOwnershipData;

    public EntitySimulation(EntityRegistry entityRegistry, WorldEntityManager worldEntityManager, SimulationOwnershipData simulationOwnershipData, PlayerManager playerManager, ISimulationWhitelist simulationWhitelist)
    {
        this.entityRegistry = entityRegistry;
        this.worldEntityManager = worldEntityManager;
        this.simulationOwnershipData = simulationOwnershipData;
        this.playerManager = playerManager;
        this.simulationWhitelist = simulationWhitelist;
    }

    public List<SimulatedEntity> GetSimulationChangesForCell(Player player, AbsoluteEntityCell cell)
    {
        List<WorldEntity> entities = worldEntityManager.GetEntities(cell);
        List<WorldEntity> addedEntities = FilterSimulatableEntities(player, entities);

        List<SimulatedEntity> ownershipChanges = new();

        foreach (WorldEntity entity in addedEntities)
        {
            bool doesEntityMove = ShouldSimulateEntityMovement(entity);
            ownershipChanges.Add(new SimulatedEntity(entity.Id, player.Id, doesEntityMove, DEFAULT_ENTITY_SIMULATION_LOCKTYPE));
        }

        return ownershipChanges;
    }

    public void FillWithRemovedCells(Player player, AbsoluteEntityCell removedCell, List<SimulatedEntity> ownershipChanges)
    {
        List<WorldEntity> entities = worldEntityManager.GetEntities(removedCell);
        IEnumerable<WorldEntity> revokedEntities = entities.Where(entity => !player.CanSee(entity) && simulationOwnershipData.RevokeIfOwner(entity.Id, player));
        AssignEntitiesToOtherPlayers(player, revokedEntities, ownershipChanges);
    }

    public void BroadcastSimulationChanges(List<SimulatedEntity> ownershipChanges)
    {
        if (ownershipChanges.Count > 0)
        {
            SimulationOwnershipChange ownershipChange = new(ownershipChanges);
            playerManager.SendPacketToAllPlayers(ownershipChange);
        }
    }

    public List<SimulatedEntity> CalculateSimulationChangesFromPlayerDisconnect(Player player)
    {
        List<SimulatedEntity> ownershipChanges = new();

        List<NitroxId> revokedEntityIds = simulationOwnershipData.RevokeAllForOwner(player);
        List<Entity> revokedEntities = entityRegistry.GetEntities(revokedEntityIds);

        AssignEntitiesToOtherPlayers(player, revokedEntities, ownershipChanges);

        return ownershipChanges;
    }

    public SimulatedEntity AssignNewEntityToPlayer(Entity entity, Player player, bool shouldEntityMove = true)
    {
        if (simulationOwnershipData.TryToAcquire(entity.Id, player, DEFAULT_ENTITY_SIMULATION_LOCKTYPE))
        {
            bool doesEntityMove = shouldEntityMove && entity is WorldEntity worldEntity && ShouldSimulateEntityMovement(worldEntity);
            return new SimulatedEntity(entity.Id, player.Id, doesEntityMove, DEFAULT_ENTITY_SIMULATION_LOCKTYPE);
        }

        throw new Exception($"New entity was already being simulated by someone else: {entity.Id}");
    }

    public List<SimulatedEntity> AssignGlobalRootEntitiesAndGetData(Player player)
    {
        List<SimulatedEntity> simulatedEntities = new();
        foreach (GlobalRootEntity entity in worldEntityManager.GetGlobalRootEntities())
        {
            simulationOwnershipData.TryToAcquire(entity.Id, player, SimulationLockType.TRANSIENT);
            bool doesEntityMove = ShouldSimulateEntityMovement(entity);
            SimulatedEntity simulatedEntity = new(entity.Id, simulationOwnershipData.GetPlayerForLock(entity.Id).Id, doesEntityMove, SimulationLockType.TRANSIENT);
            simulatedEntities.Add(simulatedEntity);
        }
        return simulatedEntities;
    }

    private void AssignEntitiesToOtherPlayers(Player oldPlayer, IEnumerable<Entity> entities, List<SimulatedEntity> ownershipChanges)
    {
        List<Player> otherPlayers = playerManager.GetConnectedPlayersExcept(oldPlayer);
        foreach (Entity entity in entities)
        {
            NitroxId id = entity.Id;
            foreach (Player player in otherPlayers)
            {
                if (player.CanSee(entity) && simulationOwnershipData.TryToAcquire(id, player, DEFAULT_ENTITY_SIMULATION_LOCKTYPE))
                {
                    bool doesEntityMove = entity is WorldEntity worldEntity && ShouldSimulateEntityMovement(worldEntity);
                    ownershipChanges.Add(new SimulatedEntity(id, player.Id, doesEntityMove, DEFAULT_ENTITY_SIMULATION_LOCKTYPE));

                    Log.Verbose($"Player {player.Name} has taken over simulating {id}");
                    break;
                }
            }
        }
    }

    private List<WorldEntity> FilterSimulatableEntities(Player player, List<WorldEntity> entities)
    {
        return entities.Where(entity => {
            bool isEligibleForSimulation = player.CanSee(entity) && ShouldSimulateEntity(entity);
            return isEligibleForSimulation && simulationOwnershipData.TryToAcquire(entity.Id, player, DEFAULT_ENTITY_SIMULATION_LOCKTYPE);
        }).ToList();
    }

    public bool ShouldSimulateEntity(WorldEntity entity)
    {
        return simulationWhitelist.UtilityWhitelist.Contains(entity.TechType) || ShouldSimulateEntityMovement(entity);
    }

    public bool ShouldSimulateEntityMovement(WorldEntity entity)
    {
        return !entity.SpawnedByServer || simulationWhitelist.MovementWhitelist.Contains(entity.TechType);    
    }

    public bool ShouldSimulateEntityMovement(NitroxId entityId)
    {
        return entityRegistry.TryGetEntityById(entityId, out WorldEntity worldEntity) && ShouldSimulateEntityMovement(worldEntity);
    }

    public void EntityDestroyed(NitroxId id)
    {
        simulationOwnershipData.RevokeOwnerOfId(id);
    }
}
