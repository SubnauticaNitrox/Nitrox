using System;
using System.Collections;
using System.Collections.Generic;
using NitroxClient.GameLogic.Spawning.Abstract;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.GameLogic.Spawning.WorldEntities;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using Nitrox.Model.Subnautica.DataStructures;
using NitroxModel.Core;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning;

public class WorldEntitySpawner : SyncEntitySpawner<WorldEntity>
{
    private readonly WorldEntitySpawnerResolver worldEntitySpawnResolver;
    private readonly Lazy<Dictionary<Int3, BatchCells>> batchCellsById = new(() =>
    {
        if (NitroxEnvironment.IsNormal)
        {
            return LargeWorldStreamer.main.cellManager.batch2cells;
        }
        return [];
    });

    public WorldEntitySpawner(EntityMetadataManager entityMetadataManager, PlayerManager playerManager, LocalPlayer localPlayer, Entities entities, SimulationOwnership simulationOwnership)
    {
        worldEntitySpawnResolver = new WorldEntitySpawnerResolver(entityMetadataManager, playerManager, localPlayer, entities, simulationOwnership);
    }

    protected override IEnumerator SpawnAsync(WorldEntity entity, TaskResult<Optional<GameObject>> result)
    {
        EntityCell cellRoot = EnsureCell(entity);
        if (cellRoot == null)
        {
            // Error logging is done in EnsureCell
            return null;
        }

        Optional<GameObject> parent = (entity.ParentId != null) ? NitroxEntity.GetObjectFrom(entity.ParentId) : Optional.Empty;

        IWorldEntitySpawner entitySpawner = worldEntitySpawnResolver.ResolveEntitySpawner(entity);

        if (entitySpawner is IWorldEntitySyncSpawner syncSpawner &&
            syncSpawner.SpawnSync(entity, parent, cellRoot, result))
        {
            return null;
        }

        return entitySpawner.SpawnAsync(entity, parent, cellRoot, result);
    }

    protected override bool SpawnsOwnChildren(WorldEntity entity)
    {
        IWorldEntitySpawner entitySpawner = worldEntitySpawnResolver.ResolveEntitySpawner(entity);
        return entitySpawner.SpawnsOwnChildren();
    }

    protected override bool SpawnSync(WorldEntity entity, TaskResult<Optional<GameObject>> result)
    {
        EntityCell cellRoot = EnsureCell(entity);
        if (cellRoot == null)
        {
            // Error logging is done in EnsureCell
            return true;
        }

        Optional<GameObject> parent = (entity.ParentId != null) ? NitroxEntity.GetObjectFrom(entity.ParentId) : Optional.Empty;
        IWorldEntitySpawner entitySpawner = worldEntitySpawnResolver.ResolveEntitySpawner(entity);

        return entitySpawner is IWorldEntitySyncSpawner syncSpawner && syncSpawner.SpawnSync(entity, parent, cellRoot, result);
    }

    public EntityCell EnsureCell(WorldEntity entity)
    {
        EntityCell entityCell;

        Int3 batchId = entity.AbsoluteEntityCell.BatchId.ToUnity();
        Int3 cellId = entity.AbsoluteEntityCell.CellId.ToUnity();

        if (!batchCellsById.Value.TryGetValue(batchId, out BatchCells batchCells))
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
