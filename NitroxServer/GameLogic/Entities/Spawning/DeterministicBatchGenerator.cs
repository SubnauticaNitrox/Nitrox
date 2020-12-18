using System;
using System.Collections.Generic;
using NitroxModel.DataStructures;

namespace NitroxServer.GameLogic.Entities.Spawning
{
    public class DeterministicBatchGenerator
    {
        private readonly Random random;
        private static readonly ThreadSafeCollection<NitroxId> nitroxIds = new ThreadSafeCollection<NitroxId>(new HashSet<NitroxId>());

        public DeterministicBatchGenerator(string seed, NitroxInt3 batchId)
        {
            random = new Random(seed.GetHashCode() + batchId.GetHashCode());
        }

        public double NextDouble()
        {
            return random.NextDouble();
        }

        public int NextInt(int min, int max)
        {
            return random.Next(min, max);
        }

        public NitroxId NextId()
        {
            byte[] bytes = new byte[16];
            random.NextBytes(bytes);
            NitroxId id = new NitroxId(bytes);

            if (nitroxIds.Contains(id))
            {
                NitroxId newId = NextId();
                nitroxIds.Add(newId);
                return newId;
            }

            nitroxIds.Add(id);
            return id;
        }
    }
}
