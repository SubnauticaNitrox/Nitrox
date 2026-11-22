using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Model.Subnautica.Helper;
using Nitrox.Server.Subnautica.Models.AppEvents;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Entities;

internal sealed class EntitySimulation : ISessionCleaner
{
    private const SimulationLockType DEFAULT_ENTITY_SIMULATION_LOCKTYPE = SimulationLockType.TRANSIENT;
    private readonly EntityRegistry entityRegistry;
    private readonly ILogger<EntitySimulation> logger;

    private readonly IPacketSender packetSender;
    private readonly PlayerManager playerManager;
    private readonly SimulationOwnershipData simulationOwnershipData;
    private readonly WorldEntityManager worldEntityManager;

    public EntitySimulation(IPacketSender packetSender, EntityRegistry entityRegistry, WorldEntityManager worldEntityManager, SimulationOwnershipData simulationOwnershipData, PlayerManager playerManager, ILogger<EntitySimulation> logger)
    {
        this.packetSender = packetSender;
        this.entityRegistry = entityRegistry;
        this.worldEntityManager = worldEntityManager;
        this.simulationOwnershipData = simulationOwnershipData;
        this.playerManager = playerManager;
        this.logger = logger;
    }

    /// <returns>The successfully acquired entities from the provided <paramref name="cell"/>.</returns>
    public IEnumerable<SimulatedEntity> TryAcquireCellEntities(Player player, AbsoluteEntityCell cell)
    {
        return TryAcquireEntities(player, worldEntityManager.EnumerateCellEntities(cell), true, true);
    }

    private IEnumerable<SimulatedEntity> TryAcquireEntities(Player player, IEnumerable<Entity> entities, bool checkPlayerCanSee, bool propagate)
    {
        foreach (Entity entity in entities)
        {
            if (!SimulationWhitelist.ShouldSimulateEntity(entity))
            {
                continue;
            }

            if (checkPlayerCanSee && !player.CanSee(entity))
            {
                continue;
            }

            foreach (SimulatedEntity simulatedEntity in TryAcquireEntity(player, entity, propagate))
            {
                yield return simulatedEntity;
            }
        }
    }

    public IEnumerable<SimulatedEntity> TryAcquireEntity(Player player, Entity entity, bool propagate, SimulationLockType simulationLockType = DEFAULT_ENTITY_SIMULATION_LOCKTYPE)
    {
        if (!simulationOwnershipData.TryAcquire(entity.Id, player, simulationLockType))
        {
            yield break;
        }

        bool doesEntityMove = entity is WorldEntity worldEntity && SimulationWhitelist.ShouldSimulateEntityMovement(worldEntity);
        yield return new SimulatedEntity(entity.Id, player.SessionId, doesEntityMove, simulationLockType);

        if (propagate && SimulationWhitelist.ShouldPropagateSimulation(entity))
        {
            foreach (SimulatedEntity simulatedEntity in TryAcquireEntities(player, entity.ChildEntities, false, propagate))
            {
                yield return simulatedEntity;
            }
        }
    }

    /// <returns>The successfully revoked simulated entities from the provided <paramref name="cell"/>.</returns>
    public IEnumerable<WorldEntity> RevokeSimulatedCellEntities(Player simulatingPlayer, AbsoluteEntityCell cell)
    {
        return RevokeEntities(simulatingPlayer, worldEntityManager.EnumerateCellEntities(cell), true);
    }

    private IEnumerable<WorldEntity> RevokeEntities(Player simulatingPlayer, IEnumerable<WorldEntity> entities, bool checkCanPlayerSee)
    {
        foreach (WorldEntity worldEntity in entities)
        {
            if (checkCanPlayerSee && simulatingPlayer.CanSee(worldEntity))
            {
                continue;
            }
            if (simulationOwnershipData.RevokeIfOwner(worldEntity.Id, simulatingPlayer))
            {
                yield return worldEntity;
            }

            if (!SimulationWhitelist.ShouldPropagateSimulation(worldEntity))
            {
                continue;
            }

            // PlayerSeeOutOfCell state might remain in a tiny time frame while player unloads cell, so we propagate without checking CanSee
            foreach (WorldEntity childWorldEntity in RevokeEntities(simulatingPlayer, worldEntity.ChildEntities.OfType<WorldEntity>(), false))
            {
                yield return childWorldEntity;
            }
        }
    }

    /// <summary>
    /// Fills <paramref name="ownershipChanges"/> with ownership changes from revoking <paramref name="player"/>'s simulated entities in <paramref name="removedCell"/>.
    /// </summary>
    public void RevokeAndReassignCellEntities(Player player, AbsoluteEntityCell removedCell, List<SimulatedEntity> ownershipChanges)
    {
        AssignEntitiesToOtherPlayers(player.SessionId, RevokeSimulatedCellEntities(player, removedCell), ownershipChanges, true);
    }

    public async Task BroadcastSimulationChanges(List<SimulatedEntity> ownershipChanges)
    {
        if (ownershipChanges.Count > 0)
        {
            SimulationOwnershipChange ownershipChange = new(ownershipChanges);
            await packetSender.SendPacketToAllAsync(ownershipChange);
        }
    }

    /// <summary>
    /// Forcefully assign an entity to a player, revoking any previous ownership.
    /// </summary>
    public IEnumerable<SimulatedEntity> AssignEntityToPlayer(WorldEntity worldEntity, Player player, bool propagate)
    {
        simulationOwnershipData.ForceAcquire(worldEntity.Id, player, DEFAULT_ENTITY_SIMULATION_LOCKTYPE);
        return TryAcquireEntity(player, worldEntity, propagate);
    }

    public List<SimulatedEntity> AssignGlobalRootEntitiesAndGetData(Player player)
    {
        List<SimulatedEntity> simulatedEntities = [];
        foreach (GlobalRootEntity entity in worldEntityManager.GetGlobalRootEntities())
        {
            simulationOwnershipData.TryAcquire(entity.Id, player, SimulationLockType.TRANSIENT);
            if (!simulationOwnershipData.TryGetLock(entity.Id, out SimulationOwnershipData.PlayerLock playerLock))
            {
                continue;
            }
            bool doesEntityMove = SimulationWhitelist.ShouldSimulateEntityMovement(entity);
            SimulatedEntity simulatedEntity = new(entity.Id, playerLock.Player.SessionId, doesEntityMove, playerLock.LockType);
            simulatedEntities.Add(simulatedEntity);
        }
        return simulatedEntities;
    }

    public bool TryAssignEntityToPlayers(List<Player> players, Entity entity, bool propagate, [NotNullWhen(true)] out List<SimulatedEntity>? simulatedEntities)
    {
        NitroxId id = entity.Id;

        foreach (Player player in players)
        {
            if (!player.CanSee(entity))
            {
                continue;
            }

            simulatedEntities = [.. TryAcquireEntity(player, entity, false)];
            if (simulatedEntities.Count == 0)
            {
                continue;
            }

            if (propagate && SimulationWhitelist.ShouldPropagateSimulation(entity))
            {
                foreach (Entity childEntity in entity.ChildEntities)
                {
                    simulatedEntities.AddRange(TryAcquireEntity(player, childEntity, false));
                }
            }

            logger.ZLogTrace($"Player {player.Name} has taken over simulating {id}");
            return true;

        }

        simulatedEntities = null;
        return false;
    }

    public bool ShouldSimulateEntityMovement(NitroxId entityId)
    {
        return entityRegistry.TryGetEntityById(entityId, out WorldEntity worldEntity) && SimulationWhitelist.ShouldSimulateEntityMovement(worldEntity);
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
        List<SimulatedEntity> ownershipChanges = [];

        List<NitroxId> revokedEntityIds = simulationOwnershipData.RevokeAllForOwner(sessionId);
        List<Entity> revokedEntities = entityRegistry.GetEntities(revokedEntityIds);

        AssignEntitiesToOtherPlayers(sessionId, revokedEntities, ownershipChanges, false);

        return ownershipChanges;
    }

    public void AssignEntitiesToOtherPlayers(SessionId oldSessionId, IEnumerable<Entity> entities, List<SimulatedEntity> ownershipChanges, bool propagate)
    {
        // In case the enumerator can be counted (e.g. a list)
        if (entities.TryGetNonEnumeratedCount(out int count))
        {
            ownershipChanges.EnsureCapacity(ownershipChanges.Count + count);
        }

        // TODO: (optional) Find out if ordering the otherPlayers by distance to the previous simulator improves performance (ascending)
        List<Player> otherPlayers = playerManager.GetConnectedPlayersExcept(oldSessionId);
        foreach (Entity entity in entities)
        {
            if (TryAssignEntityToPlayers(otherPlayers, entity, propagate, out List<SimulatedEntity> simulatedEntities))
            {
                ownershipChanges.AddRange(simulatedEntities);
                continue;
            }

            ownershipChanges.Add(new(entity.Id, SessionId.SERVER_ID, false, SimulationLockType.TRANSIENT));
            if (propagate && SimulationWhitelist.ShouldPropagateSimulation(entity))
            {
                foreach (Entity childEntity in entity.ChildEntities)
                {
                    if (simulationOwnershipData.GetPlayerForLock(childEntity.Id) == null)
                    {
                        ownershipChanges.Add(new(childEntity.Id, SessionId.SERVER_ID, false, SimulationLockType.TRANSIENT));
                    }
                }
            }
        }
    }
}
