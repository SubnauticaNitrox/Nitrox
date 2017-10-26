using System;

namespace NitroxModel.DataStructures
{
    [Serializable]
    public class Chunk
    {
        public Int3 BatchId { get; }
        public int Level { get; } // 0-3 lower means 'closer to the player' and 'higher level-of-detail'

        public Chunk(Int3 batchId, int level)
        {
            BatchId = batchId;
            Level = level;
        }

        public override string ToString()
        {
            return "[Chunk " + BatchId + " " + Level + "]";
        }

        public override bool Equals(Object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Chunk chunk = (Chunk)obj;

            return (chunk.Level == Level &&
                    chunk.BatchId.x == BatchId.x &&
                    chunk.BatchId.y == BatchId.y &&
                    chunk.BatchId.z == BatchId.z);
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
                return hash;
            }
        }

    }
}
