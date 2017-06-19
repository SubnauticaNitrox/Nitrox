using NitroxModel.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxClient.Map
{
    public class LoadedChunks
    {
        private static int CHUNK_SIZE = 16;

        private HashSet<Int3> chunks = new HashSet<Int3>();

        public void AddChunk(Int3 position)
        {
            chunks.Add(position);
        }

        public void RemoveChunk(Int3 position)
        {
            chunks.Remove(position);
        }

        public bool IsLoadedChunk(Int3 chunk)
        {
            return chunks.Contains(chunk);
        }

        public Int3 GetChunk(Vector3 position)
        {
            return new Int3((int)Math.Floor(position.X / CHUNK_SIZE) * CHUNK_SIZE,
                            (int)Math.Floor(position.Y / CHUNK_SIZE) * CHUNK_SIZE,
                            (int)Math.Floor(position.Z / CHUNK_SIZE) * CHUNK_SIZE);
        }
    }
}
