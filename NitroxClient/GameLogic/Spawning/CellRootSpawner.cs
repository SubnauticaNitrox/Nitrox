using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using UnityEngine;
using NitroxInt3 = NitroxModel.DataStructures.Int3;

namespace NitroxClient.GameLogic.Spawning
{
    public class CellRootSpawner : IEntitySpawner
    {
        public Optional<GameObject> Spawn(Entity entity, Optional<GameObject> parent, EntityCell cellRoot)
        {
            NitroxInt3 cellId = entity.AbsoluteEntityCell.CellId;
            NitroxInt3 batchId = entity.AbsoluteEntityCell.BatchId;

            cellRoot.liveRoot.name = $"CellRoot {cellId.X}, {cellId.Y}, {cellId.Z}; Batch {batchId.X}, {batchId.Y}, {batchId.Z}";

            NitroxEntity.SetNewId(cellRoot.liveRoot, entity.Id);

            LargeWorldStreamer.main.cellManager.QueueForAwake(cellRoot);

            return Optional.OfNullable(cellRoot.liveRoot);
        }

        public bool SpawnsOwnChildren()
        {
            return false;
        }
    }
}
