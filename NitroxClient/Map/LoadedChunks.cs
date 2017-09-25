using System.Collections.Generic;
using UnityEngine;

namespace NitroxClient.Map
{
    public class LoadedChunks : HashSet<Int3>
    {
        public Int3 GetChunk(Vector3 position)
        {
            return LargeWorldStreamer.main.GetContainingBatch(position);
        }
    }
}
