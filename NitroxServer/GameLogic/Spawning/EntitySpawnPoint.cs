using NitroxModel.Helper;
using NitroxServer.UnityStubs;

namespace NitroxServer.GameLogic.Spawning
{
    public class EntitySpawnPoint
    {
        public Int3 BatchId { get; private set; }
        public Int3 CellId { get; private set; }
        public UnityEngine.Vector3 Position { get; private set; }
        public UnityEngine.Quaternion Rotation { get; private set; }
        public int Level { get; private set; }
        public string ClassId { get; private set; }
        public string Guid { get; private set; }
        public BiomeType BiomeType { get; private set; }
        public float Density { get; private set; }
        public bool CanSpawnCreature { get; private set; }

        public static EntitySpawnPoint From(Int3 batchId, GameObject go, CellManager.CellHeader cellHeader)
        {
            // Why is this not a constructor?
            EntitySpawnPoint esp = new EntitySpawnPoint
            {
                Level = cellHeader.level,
                ClassId = go.ClassId,
                BatchId = batchId,
                CellId = cellHeader.cellId
            };

            EntitySlot entitySlot = go.GetComponent<EntitySlot>();

            if (!ReferenceEquals(entitySlot, null))
            {
                esp.BiomeType = entitySlot.biomeType;
                esp.Density = entitySlot.density;
                esp.CanSpawnCreature = entitySlot.IsCreatureSlot();
            }

            esp.Rotation = go.GetComponent<Transform>().Rotation;

            Int3.Bounds bounds = BatchCells.GetBlockBounds(batchId, cellHeader.cellId, esp.Level, Map.BATCH_DIMENSIONS);
            UnityEngine.Vector3 localPosition = go.GetComponent<Transform>().Position;
            UnityEngine.Vector3 center = EntityCell.GetCenter(bounds);
            esp.Position = center + localPosition - Map.BATCH_DIMENSION_CENTERING.ToVector3();

            return esp;
        }
    }
}
