using NitroxModel.Helper;
using NitroxServer.UnityStubs;
using System;

namespace NitroxServer.GameLogic.Spawning
{
    public class EntitySpawnPoint
    {
        public Int3 BatchId { get; private set; }
        public Int3 CellId { get; private set; }
        public UnityEngine.Vector3 Position { get; private set; }        
        public int Level { get; private set; }
        public String ClassId { get; private set; }
        public String Guid { get; private set; }
        public BiomeType BiomeType { get; private set; }
        public float Density { get; private set; }
        public bool CanSpawnCreature { get; private set; }

        public static EntitySpawnPoint From(Int3 batchId, GameObject go, CellManager.CellHeader cellHeader)
        {
            EntitySpawnPoint esp = new EntitySpawnPoint();
            esp.Level = cellHeader.level;
            esp.ClassId = go.ClassId;
            esp.BatchId = batchId;
            esp.CellId = cellHeader.cellId;

            EntitySlot entitySlot = go.GetComponent<EntitySlot>();

            if (!ReferenceEquals(entitySlot, null))
            {
                esp.BiomeType = entitySlot.biomeType;
                esp.Density = entitySlot.density;
                esp.CanSpawnCreature = entitySlot.IsCreatureSlot();
            }

            Vector3 localPosition = go.GetComponent<Transform>().Position;
            Int3.Bounds bounds = BatchCells.GetBlockBounds(batchId, cellHeader.cellId, 0, Map.BATCH_DIMENSIONS);
            UnityEngine.Vector3 center = EntityCell.GetCenter(bounds);
            
            esp.Position = new UnityEngine.Vector3(center.x + localPosition.x - Map.BATCH_DIMENSION_CENTERING.x,
                                                   center.y + localPosition.y - Map.BATCH_DIMENSION_CENTERING.y,
                                                   center.z + localPosition.z - Map.BATCH_DIMENSION_CENTERING.z); 
            return esp;
        }
    }
}
