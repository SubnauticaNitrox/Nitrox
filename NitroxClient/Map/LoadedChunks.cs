using System;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxClient.Map
{
    public class LoadedChunks : HashSet<Int3>
    {
        private static int CHUNK_SIZE = 16;

        public Int3 GetChunk(Vector3 position)
        {
            return new Int3((int)Math.Floor(position.x / CHUNK_SIZE) * CHUNK_SIZE,
                            (int)Math.Floor(position.y / CHUNK_SIZE) * CHUNK_SIZE,
                            (int)Math.Floor(position.z / CHUNK_SIZE) * CHUNK_SIZE);
        }
    }
}
