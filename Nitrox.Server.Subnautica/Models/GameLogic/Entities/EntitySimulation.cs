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
        foreach (WorldEntity worldEntity in worldEntityManager.EnumerateCellEntities(cell))
        {
            if (!player.CanSee(worldEntity) || !SimulationWhitelist.ShouldSimulateEntity(worldEntity))
            {
                continue;
            }

            if (simulationOwnershipData.TryAcquire(worldEntity.Id, player, DEFAULT_ENTITY_SIMULATION_LOCKTYPE))
            {
                bool doesEntityMove = SimulationWhitelist.ShouldSimulateEntityMovement(worldEntity);
                yield return new SimulatedEntity(worldEntity.Id, player.SessionId, doesEntityMove, DEFAULT_ENTITY_SIMULATION_LOCKTYPE);
            }
        }
    }

    /// <returns>The successfully revoked simulated entities from the provided <paramref name="cell"/>.</returns>
    public IEnumerable<WorldEntity> RevokeSimulatedCellEntities(Player simulatingPlayer, AbsoluteEntityCell cell)
    {
        foreach (WorldEntity entity in worldEntityManager.EnumerateCellEntities(cell))
        {
            if (simulatingPlayer.CanSee(entity))
            {
                continue;
            }
            if (simulationOwnershipData.RevokeIfOwner(entity.Id, simulatingPlayer))
            {
                yield return entity;
            }
        }
    }

    /// <summary>
    /// Fills <paramref name="ownershipChanges"/> with ownership changes from revoking <paramref name="player"/>'s simulated entities in <paramref name="removedCell"/>.
    /// </summary>
    public void RevokeAndReassignCellEntities(Player player, AbsoluteEntityCell removedCell, List<SimulatedEntity> ownershipChanges)
    {
        AssignEntitiesToOtherPlayers(player.SessionId, RevokeSimulatedCellEntities(player, removedCell), ownershipChanges);
    }

    public void BroadcastSimulationChanges(List<SimulatedEntity> ownershipChanges)
    {
        if (ownershipChanges.Count > 0)
        {
            SimulationOwnershipChange ownershipChange = new(ownershipChanges);
            packetSender.SendPacketToAllAsync(ownershipChange);
        }
    }

    public bool TryAssignEntityToPlayer(Entity entity, Player player, bool shouldEntityMove, [NotNullWhen(true)] out SimulatedEntity? simulatedEntity)
    {
        if (simulationOwnershipData.TryAcquire(entity.Id, player, DEFAULT_ENTITY_SIMULATION_LOCKTYPE))
        {
            bool doesEntityMove = shouldEntityMove && entity is WorldEntity worldEntity && SimulationWhitelist.ShouldSimulateEntityMovement(worldEntity);
            simulatedEntity = new(entity.Id, player.SessionId, doesEntityMove, DEFAULT_ENTITY_SIMULATION_LOCKTYPE);
            return true;
        }

        simulatedEntity = null;
        return false;
    }

    /// <summary>
    /// Forcefully assign an entity to a player, revoking any previous ownership.
    /// </summary>
    public SimulatedEntity AssignEntityToPlayer(Entity entity, Player player, bool shouldEntityMove)
    {
        simulationOwnershipData.ForceAcquire(entity.Id, player, DEFAULT_ENTITY_SIMULATION_LOCKTYPE);
        bool doesEntityMove = shouldEntityMove && entity is WorldEntity worldEntity && SimulationWhitelist.ShouldSimulateEntityMovement(worldEntity);
        return new(entity.Id, player.SessionId, doesEntityMove, DEFAULT_ENTITY_SIMULATION_LOCKTYPE);
    }

    public List<SimulatedEntity> AssignGlobalRootEntitiesAndGetData(Player player)
    {
        List<SimulatedEntity> simulatedEntities = new();
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

    public bool TryAssignEntityToPlayers(List<Player> players, Entity entity, [NotNullWhen(true)] out SimulatedEntity? simulatedEntity)
    {
        NitroxId id = entity.Id;

        foreach (Player player in players)
        {
            if (player.CanSee(entity) && simulationOwnershipData.TryAcquire(id, player, DEFAULT_ENTITY_SIMULATION_LOCKTYPE))
            {
                bool doesEntityMove = entity is WorldEntity worldEntity && SimulationWhitelist.ShouldSimulateEntityMovement(worldEntity);

                logger.ZLogTrace($"Player {player.Name} has taken over simulating {id}");
                simulatedEntity = new(id, player.SessionId, doesEntityMove, DEFAULT_ENTITY_SIMULATION_LOCKTYPE);
                return true;
            }
        }

        simulatedEntity = null;
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
        List<SimulatedEntity> ownershipChanges = new();

        List<NitroxId> revokedEntityIds = simulationOwnershipData.RevokeAllForOwner(sessionId);
        List<Entity> revokedEntities = entityRegistry.GetEntities(revokedEntityIds);

        AssignEntitiesToOtherPlayers(sessionId, revokedEntities, ownershipChanges);

        return ownershipChanges;
    }

    public void AssignEntitiesToOtherPlayers(SessionId oldSessionId, IEnumerable<Entity> entities, List<SimulatedEntity> ownershipChanges)
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
            if (TryAssignEntityToPlayers(otherPlayers, entity, out SimulatedEntity simulatedEntity))
            {
                ownershipChanges.Add(simulatedEntity);
            }
            else
            {
                ownershipChanges.Add(new(entity.Id, SessionId.SERVER_ID, false, SimulationLockType.TRANSIENT));
            }
        }
    }
}
