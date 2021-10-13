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
#if SUBNAUTICA
        public Optional<GameObject> Spawn(Entity entity, Optional<GameObject> parent, EntityCell cellRoot)
#elif BELOWZERO
        public IEnumerator Spawn(TaskResult<Optional<GameObject>> result, Entity entity, Optional<GameObject> parent, EntityCell cellRoot)
#endif
        {
            NitroxInt3 cellId = entity.AbsoluteEntityCell.CellId;
            NitroxInt3 batchId = entity.AbsoluteEntityCell.BatchId;

            cellRoot.liveRoot.name = $"CellRoot {cellId.X}, {cellId.Y}, {cellId.Z}; Batch {batchId.X}, {batchId.Y}, {batchId.Z}";

            NitroxEntity.SetNewId(cellRoot.liveRoot, entity.Id);

            LargeWorldStreamer.main.cellManager.QueueForAwake(cellRoot);
#if SUBNAUTICA
            return Optional.OfNullable(cellRoot.liveRoot);
#elif BELOWZERO
            result.Set(Optional.OfNullable(cellRoot.liveRoot));
            yield break;
#endif
        }

        public bool SpawnsOwnChildren()
        {
            return false;
        }
    }
}
