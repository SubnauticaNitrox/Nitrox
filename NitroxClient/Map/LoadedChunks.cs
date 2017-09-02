using NitroxModel.DataStructures;
using System;
using System.Collections.Generic;

namespace NitroxClient.Map
{
    public class LoadedChunks : HashSet<Int3>
    {
        private static int CHUNK_SIZE = 16;

        public Int3 GetChunk(Vector3 position)
        {
            return new Int3((int)Math.Floor(position.X / CHUNK_SIZE) * CHUNK_SIZE,
                            (int)Math.Floor(position.Y / CHUNK_SIZE) * CHUNK_SIZE,
                            (int)Math.Floor(position.Z / CHUNK_SIZE) * CHUNK_SIZE);
        }
    }
}
