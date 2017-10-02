using System;

namespace NitroxModel.DataStructures
{
    [Serializable]
    public class Chunk
    {
        public Int3 BatchId { get { return serializableBatchId.toInt3(); } }
        public int Level { get; }

        public SerializableInt3 serializableBatchId { get; }

        public Chunk(Int3 batchId, int level)
        {
            serializableBatchId = SerializableInt3.from(batchId);
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
                return false;

            Chunk chunk = (Chunk)obj;

            return (chunk.Level == this.Level &&
                    chunk.serializableBatchId.X == this.serializableBatchId.X &&
                    chunk.serializableBatchId.Y == this.serializableBatchId.Y &&
                    chunk.serializableBatchId.Z == this.serializableBatchId.Z);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 269;
                hash = hash * 23 + Level;
                hash = hash * 23 + serializableBatchId.X.GetHashCode();
                hash = hash * 23 + serializableBatchId.Y.GetHashCode();
                hash = hash * 23 + serializableBatchId.Z.GetHashCode();
                return hash;
            }
        }

    }
}
