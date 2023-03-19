using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxClient.Map
{
    public class VisibleBatches
    {
        private readonly HashSet<NitroxInt3> batchIds = new HashSet<NitroxInt3>();

        public void Add(NitroxInt3 batchId)
        {
            lock (batchIds)
            {
                batchIds.Add(batchId);
            }
        }

        public void Remove(NitroxInt3 batchId)
        {
            lock (batchIds)
            {
                batchIds.Remove(batchId);
            }
        }

        public bool Contains(NitroxInt3 batchId)
        {
            lock (batchIds)
            {
                return batchIds.Contains(batchId);
            }
        }
    }
}
