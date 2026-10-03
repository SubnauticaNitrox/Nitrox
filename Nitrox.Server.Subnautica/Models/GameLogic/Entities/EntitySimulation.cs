using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.AppEvents;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Connected;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Entities;

internal sealed class EntitySimulation(IPacketSender packetSender, EntityRegistry entityRegistry, WorldEntityManager worldEntityManager, SimulationOwnershipData simulationOwnershipData, PlayerService playerService, ILogger<EntitySimulation> logger)
    : ISessionCleaner
{
    private const SimulationLockType DEFAULT_ENTITY_SIMULATION_LOCKTYPE = SimulationLockType.TRANSIENT;
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly ILogger<EntitySimulation> logger = logger;

    private readonly IPacketSender packetSender = packetSender;
    private readonly PlayerService playerService = playerService;
    private readonly SimulationOwnershipData simulationOwnershipData = simulationOwnershipData;
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;

    public IEnumerable<SimulatedEntity> GetSimulationChangesForCell(SessionId sessionId, AbsoluteEntityCell cell)
    {
        foreach (WorldEntity entity in GetPlayerSimulatedEntities(sessionId, cell))
        {
            bool doesEntityMove = ShouldSimulateEntityMovement(entity);
            yield return new SimulatedEntity(entity.Id, sessionId, doesEntityMove, DEFAULT_ENTITY_SIMULATION_LOCKTYPE);
        }
    }

    public void FillWithRemovedCells(SessionId sessionId, AbsoluteEntityCell removedCell, List<SimulatedEntity> ownershipChanges)
    {
        AssignEntitiesToOtherPlayers(sessionId, GetEntitiesToRevoke(sessionId, removedCell), ownershipChanges);
        return;

        IEnumerable<WorldEntity> GetEntitiesOfCell(AbsoluteEntityCell cell)
        {
            foreach (WorldEntity entity in worldEntityManager.GetEntities(cell))
            {
                yield return entity;
                foreach (WorldEntity child in GetSimulatableChildren(entity))
                {
                    yield return child;
                }
            }
        }

        IEnumerable<WorldEntity> GetEntitiesToRevoke(SessionId simulatingPlayer, AbsoluteEntityCell cell)
        {
            foreach (WorldEntity entity in GetEntitiesOfCell(cell))
            {
                if (playerService.GetProperty<VisibleCellsProperty>(sessionId).CanSee(entity))
                {
                    continue;
                }
                if (!simulationOwnershipData.RevokeIfOwner(entity.Id, simulatingPlayer))
                {
                    continue;
                }

                yield return entity;
            }
        }
    }

    private IEnumerable<WorldEntity> GetSimulatableChildren(WorldEntity entity)
    {
        return entity.ChildEntities.OfType<WorldEntity>().Where(ShouldSimulateEntity);
    }

    private IEnumerable<WorldEntity> GetPlayerSimulatedEntities(SessionId simulatingPlayer, AbsoluteEntityCell cell)
    {
        foreach (WorldEntity entity in worldEntityManager.GetEntities(cell))
        {
            if (!playerService.GetProperty<VisibleCellsProperty>(simulatingPlayer).CanSee(entity))
            {
                continue;
            }
            if (!ShouldSimulateEntity(entity))
            {
                continue;
            }
            if (!simulationOwnershipData.TryToAcquire(entity.Id, simulatingPlayer, DEFAULT_ENTITY_SIMULATION_LOCKTYPE))
            {
                continue;
            }

            yield return entity;
            foreach (WorldEntity child in GetSimulatableChildren(entity))
            {
                if (simulationOwnershipData.TryToAcquire(child.Id, simulatingPlayer, DEFAULT_ENTITY_SIMULATION_LOCKTYPE))
                {
                    yield return child;
                }
            }
        }
    }

    public void BroadcastSimulationChanges(List<SimulatedEntity> ownershipChanges)
    {
        if (ownershipChanges.Count > 0)
        {
            SimulationOwnershipChange ownershipChange = new(ownershipChanges);
            packetSender.SendPacketToAllAsync(ownershipChange);
        }
    }

    public bool TryAssignEntityToPlayer(Entity entity, SessionId sessionId, bool shouldEntityMove, [NotNullWhen(true)] out SimulatedEntity? simulatedEntity)
    {
        if (simulationOwnershipData.TryToAcquire(entity.Id, sessionId, DEFAULT_ENTITY_SIMULATION_LOCKTYPE))
        {
            bool doesEntityMove = shouldEntityMove && entity is WorldEntity worldEntity && ShouldSimulateEntityMovement(worldEntity);
            simulatedEntity = new(entity.Id, sessionId, doesEntityMove, DEFAULT_ENTITY_SIMULATION_LOCKTYPE);
            return true;
        }

        simulatedEntity = null;
        return false;
    }

    public List<SimulatedEntity> AssignGlobalRootEntitiesAndGetData(SessionId sessionId)
    {
        List<SimulatedEntity> simulatedEntities = new();
        foreach (GlobalRootEntity entity in worldEntityManager.GetGlobalRootEntities())
        {
            simulationOwnershipData.TryToAcquire(entity.Id, sessionId, SimulationLockType.TRANSIENT);
            if (!simulationOwnershipData.TryGetLock(entity.Id, out SimulationOwnershipData.PlayerLock playerLock))
            {
                continue;
            }
            bool doesEntityMove = ShouldSimulateEntityMovement(entity);
            SimulatedEntity simulatedEntity = new(entity.Id, playerLock.SessionId, doesEntityMove, playerLock.LockType);
            simulatedEntities.Add(simulatedEntity);
        }
        return simulatedEntities;
    }

    public bool TryAssignEntityToPlayers(IEnumerable<SessionId> sessions, Entity entity, [NotNullWhen(true)] out SimulatedEntity? simulatedEntity)
    {
        NitroxId id = entity.Id;

        foreach (SessionId sessionId in sessions)
        {
            if (playerService.GetProperty<VisibleCellsProperty>(sessionId).CanSee(entity) && simulationOwnershipData.TryToAcquire(id, sessionId, DEFAULT_ENTITY_SIMULATION_LOCKTYPE))
            {
                bool doesEntityMove = entity is WorldEntity worldEntity && ShouldSimulateEntityMovement(worldEntity);

                logger.ZLogTrace($"Player {playerService.GetProperty<NameProperty>(sessionId).Value} has taken over simulating {id}");
                simulatedEntity = new(id, sessionId, doesEntityMove, DEFAULT_ENTITY_SIMULATION_LOCKTYPE);
                return true;
            }
        }

        simulatedEntity = null;
        return false;
    }

    public bool ShouldSimulateEntity(WorldEntity entity)
    {
        return SimulationWhitelist.UtilityWhitelist.Contains(entity.TechType) || ShouldSimulateEntityMovement(entity);
    }

    public bool ShouldSimulateEntityMovement(WorldEntity entity)
    {
        return !entity.SpawnedByServer || SimulationWhitelist.MovementWhitelist.Contains(entity.TechType);
    }

    public bool ShouldSimulateEntityMovement(NitroxId entityId)
    {
        return entityRegistry.TryGetEntityById(entityId, out WorldEntity worldEntity) && ShouldSimulateEntityMovement(worldEntity);
    }

    public void EntityDestroyed(NitroxId id)
    {
        simulationOwnershipData.RevokeOwnerOfId(id);
    }

    public async Task OnEventAsync(ISessionCleaner.Args args)
    {
        List<SimulatedEntity> ownershipChanges = CalculateSimulationChangesFromPlayerDisconnect(args.Session.Id);
        if (ownershipChanges.Count > 0)
        {
            SimulationOwnershipChange ownershipChange = new(ownershipChanges);
            await packetSender.SendPacketToAllAsync(ownershipChange);
        }
    }

    private List<SimulatedEntity> CalculateSimulationChangesFromPlayerDisconnect(SessionId sessionId)
    {
        List<SimulatedEntity> ownershipChanges = new();

        List<NitroxId> revokedEntityIds = simulationOwnershipData.RevokeAllForOwner(sessionId);
        List<Entity> revokedEntities = entityRegistry.GetEntities(revokedEntityIds);

        AssignEntitiesToOtherPlayers(sessionId, revokedEntities, ownershipChanges);

        return ownershipChanges;
    }

    private void AssignEntitiesToOtherPlayers(SessionId oldSessionId, IEnumerable<Entity> entities, List<SimulatedEntity> ownershipChanges)
    {
        SessionId[] otherPlayers = playerService.GetSessionsExcept(oldSessionId).ToArray();
        foreach (Entity entity in entities)
        {
            if (TryAssignEntityToPlayers(otherPlayers, entity, out SimulatedEntity simulatedEntity))
            {
                ownershipChanges.Add(simulatedEntity);
            }
        }
    }
}
