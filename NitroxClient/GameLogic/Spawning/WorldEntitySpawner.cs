using System;
using System.Collections;
using System.Collections.Generic;
using NitroxClient.GameLogic.Spawning.Abstract;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.GameLogic.Spawning.WorldEntities;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning;

public class WorldEntitySpawner : SyncEntitySpawner<WorldEntity>
{
    private readonly WorldEntitySpawnerResolver worldEntitySpawnResolver;
    private readonly Dictionary<Int3, BatchCells> batchCellsById;

    public WorldEntitySpawner(EntityMetadataManager entityMetadataManager, PlayerManager playerManager, LocalPlayer localPlayer, Entities entities, SimulationOwnership simulationOwnership)
    {
        worldEntitySpawnResolver = new WorldEntitySpawnerResolver(entityMetadataManager, playerManager, localPlayer, entities, simulationOwnership);

        if (NitroxEnvironment.IsNormal)
        {
            batchCellsById = (Dictionary<Int3, BatchCells>)LargeWorldStreamer.main.cellManager.batch2cells;
        }
    }

    protected override IEnumerator SpawnAsync(WorldEntity entity, TaskResult<Optional<GameObject>> result)
    {
        bool foundParentCell = TryFindAwakeParentCell(entity, out EntityCell parentCell);
        if (foundParentCell)
        {
            parentCell.EnsureRoot();
        }
        Optional<GameObject> parent = (entity.ParentId != null) ? NitroxEntity.GetObjectFrom(entity.ParentId) : Optional.Empty;

        // No place to spawn the entity
        if (!foundParentCell && !parent.HasValue)
        {
            return null;
        }

        IWorldEntitySpawner entitySpawner = worldEntitySpawnResolver.ResolveEntitySpawner(entity);

        if (entitySpawner is IWorldEntitySyncSpawner syncSpawner &&
            syncSpawner.SpawnSync(entity, parent, parentCell, result))
        {
            return null;
        }

        return entitySpawner.SpawnAsync(entity, parent, parentCell, result);
    }

    protected override bool SpawnsOwnChildren(WorldEntity entity)
    {
        IWorldEntitySpawner entitySpawner = worldEntitySpawnResolver.ResolveEntitySpawner(entity);
        return entitySpawner.SpawnsOwnChildren();
    }

    protected override bool SpawnSync(WorldEntity entity, TaskResult<Optional<GameObject>> result)
    {
        bool foundParentCell = TryFindAwakeParentCell(entity, out EntityCell parentCell);
        if (foundParentCell)
        {
            parentCell.EnsureRoot();
        }
        Optional<GameObject> parent = (entity.ParentId != null) ? NitroxEntity.GetObjectFrom(entity.ParentId) : Optional.Empty;

        // No place to spawn the entity
        if (!foundParentCell && !parent.HasValue)
        {
            return true;
        }

        IWorldEntitySpawner entitySpawner = worldEntitySpawnResolver.ResolveEntitySpawner(entity);

        return entitySpawner is IWorldEntitySyncSpawner syncSpawner && syncSpawner.SpawnSync(entity, parent, parentCell, result);
    }

    public bool TryFindAwakeParentCell(WorldEntity entity, out EntityCell parentCell)
    {
        Int3 batchId = entity.AbsoluteEntityCell.BatchId.ToUnity();
        Int3 cellId = entity.AbsoluteEntityCell.CellId.ToUnity();

        if (batchCellsById.TryGetValue(batchId, out BatchCells batchCells))
        {
            parentCell = batchCells.Get(cellId, entity.Level);
            // in both states, the cell is awake
            return parentCell != null &&
                   (parentCell.state == EntityCell.State.IsAwake || parentCell.state == EntityCell.State.QueuedForSleep);
        }

        parentCell = null;
        return false;
    }

    public EntityCell EnsureCell(WorldEntity entity)
    {
        EntityCell entityCell;

        Int3 batchId = entity.AbsoluteEntityCell.BatchId.ToUnity();
        Int3 cellId = entity.AbsoluteEntityCell.CellId.ToUnity();

        if (!batchCellsById.TryGetValue(batchId, out BatchCells batchCells))
        {
            batchCells = LargeWorldStreamer.main.cellManager.InitializeBatchCells(batchId);
        }

        try
        {
            entityCell = batchCells.EnsureCell(cellId, entity.Level);
        }
        catch (Exception)
        {
            // Error logging is done in BatchCells.EnsureCell
            return null;
        }

        entityCell.EnsureRoot();

        return entityCell;
    }
}
