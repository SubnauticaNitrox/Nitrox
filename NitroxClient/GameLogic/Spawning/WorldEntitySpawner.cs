using System.Collections;
using System.Collections.Generic;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using NitroxClient.GameLogic.Spawning.WorldEntities;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning
{
    public class WorldEntitySpawner : EntitySpawner<WorldEntity>
    {
        private readonly WorldEntitySpawnerResolver worldEntitySpawnResolver;
        private readonly Dictionary<Int3, BatchCells> batchCellsById;

        public WorldEntitySpawner(PlayerManager playerManager, ILocalNitroxPlayer localPlayer)
        {
            worldEntitySpawnResolver = new WorldEntitySpawnerResolver(playerManager, localPlayer);

            if (NitroxEnvironment.IsNormal)
            {
                batchCellsById = (Dictionary<Int3, BatchCells>)LargeWorldStreamer.main.cellManager.batch2cells;
            }
        }

        public override IEnumerator SpawnAsync(WorldEntity entity, TaskResult<Optional<GameObject>> result)
        {
            LargeWorldStreamer.main.cellManager.UnloadBatchCells(entity.AbsoluteEntityCell.CellId.ToUnity()); // Just in case

            EntityCell cellRoot = EnsureCell(entity);

            Optional<GameObject> parent = (entity.ParentId != null) ? NitroxEntity.GetObjectFrom(entity.ParentId) : Optional.Empty;

            IWorldEntitySpawner entitySpawner = worldEntitySpawnResolver.ResolveEntitySpawner(entity);

            return entitySpawner.SpawnAsync(entity, parent, cellRoot, result);
        }
        public override bool SpawnsOwnChildren(WorldEntity entity)
        {
            IWorldEntitySpawner entitySpawner = worldEntitySpawnResolver.ResolveEntitySpawner(entity);
            return entitySpawner.SpawnsOwnChildren();
        }

        private EntityCell EnsureCell(WorldEntity entity)
        {
            EntityCell entityCell;

            Int3 batchId = entity.AbsoluteEntityCell.BatchId.ToUnity();
            Int3 cellId = entity.AbsoluteEntityCell.CellId.ToUnity();

            if (!batchCellsById.TryGetValue(batchId, out BatchCells batchCells))
            {
                batchCells = LargeWorldStreamer.main.cellManager.InitializeBatchCells(batchId);
            }

            entityCell = batchCells.Get(cellId, entity.AbsoluteEntityCell.Level);

            if (entityCell == null)
            {
                entityCell = batchCells.Add(cellId, entity.AbsoluteEntityCell.Level);
                entityCell.Initialize();
            }

            entityCell.EnsureRoot();

            return entityCell;
        }
    }
}
