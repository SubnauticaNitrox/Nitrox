using System;

namespace NitroxModel.DataStructures
{
    [Serializable]
    public class Chunk
    {
        public Int3 BatchId { get; }
        public Int3 CellId { get; }
        public int Level { get; } // 0-3 lower means 'closer to the player' and 'higher level-of-detail'
        
        public Chunk(Int3 batchId, Int3 cellId, int level)
        {
            BatchId = batchId;
            CellId = cellId;
            Level = level;
        }

        public override string ToString()
        {
            return "[Chunk " + BatchId + " " + CellId + " " + Level + "]";
        }

        public override bool Equals(Object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Chunk chunk = (Chunk)obj;

            return (chunk.Level == this.Level &&
                    chunk.BatchId.x == this.BatchId.x &&
                    chunk.BatchId.y == this.BatchId.y &&
                    chunk.BatchId.z == this.BatchId.z &&
                    chunk.CellId.x == this.CellId.x &&
                    chunk.CellId.y == this.CellId.y &&
                    chunk.CellId.z == this.CellId.z);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 269;
                hash = hash * 23 + Level;
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
