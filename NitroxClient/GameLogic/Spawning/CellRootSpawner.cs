using System.Collections;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning
{
    public class CellRootSpawner : IEntitySpawner
    {
        public IEnumerator SpawnAsync(Entity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result)
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
