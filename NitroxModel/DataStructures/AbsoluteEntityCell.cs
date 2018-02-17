using System;
using NitroxModel.Helper;

namespace NitroxModel.DataStructures
{
    [Serializable]
    public class AbsoluteEntityCell
    {
        public Int3 Position => (BatchId * Map.CELLS_PER_BATCH) + CellId;
        public Int3 BatchId { get; }
        public Int3 CellId { get; }

        public AbsoluteEntityCell(Int3 batchId, Int3 cellId)
        {
            BatchId = batchId;
            CellId = cellId;
        }

        public AbsoluteEntityCell(UnityEngine.Vector3 worldSpace)
        {
            float x = (worldSpace.x + Map.BATCH_DIMENSION_CENTERING.x) / Map.BATCH_SIZE;
            float y = (worldSpace.y + Map.BATCH_DIMENSION_CENTERING.y) / Map.BATCH_SIZE;
            float z = (worldSpace.z + Map.BATCH_DIMENSION_CENTERING.z) / Map.BATCH_SIZE;

            BatchId = new Int3((int)x, (int)y, (int)z);
            CellId = new Int3((int)((x - BatchId.x) * 10),
                              (int)((y - BatchId.y) * 10),
                              (int)((z - BatchId.z) * 10));
        }

        public override string ToString()
        {
            return "[AbsoluteEntityCell Position: " + Position + " BatchId: " + BatchId + " CellId: " + CellId + " ]";
        }

        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            AbsoluteEntityCell cell = (AbsoluteEntityCell)obj;

            return (cell.BatchId.x == BatchId.x &&
                    cell.BatchId.y == BatchId.y &&
                    cell.BatchId.z == BatchId.z &&
                    cell.CellId.x == CellId.x &&
                    cell.CellId.y == CellId.y &&
                    cell.CellId.z == CellId.z);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 269;
                hash = hash * 23 + BatchId.x.GetHashCode();
                hash = hash * 23 + BatchId.y.GetHashCode();
                hash = hash * 23 + BatchId.z.GetHashCode();
                hash = hash * 23 + CellId.x.GetHashCode();
                hash = hash * 23 + CellId.y.GetHashCode();
                hash = hash * 23 + CellId.z.GetHashCode();
                return hash;
            }
        }
    }
}
