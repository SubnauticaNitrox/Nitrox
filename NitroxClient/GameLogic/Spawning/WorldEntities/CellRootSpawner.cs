using System.Collections;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.WorldEntities
{
    public class CellRootSpawner : IWorldEntitySpawner
    {
        public IEnumerator SpawnAsync(WorldEntity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result)
        {
            NitroxInt3 cellId = entity.AbsoluteEntityCell.CellId;
            NitroxInt3 batchId = entity.AbsoluteEntityCell.BatchId;

            cellRoot.liveRoot.name = $"CellRoot {cellId.X}, {cellId.Y}, {cellId.Z}; Batch {batchId.X}, {batchId.Y}, {batchId.Z}";

            NitroxEntity.SetNewId(cellRoot.liveRoot, entity.Id);

            LargeWorldStreamer.main.cellManager.QueueForAwake(cellRoot);

            result.Set(Optional.OfNullable(cellRoot.liveRoot));
            yield break;
        }

        public bool SpawnsOwnChildren()
        {
            return false;
        }
    }
}
