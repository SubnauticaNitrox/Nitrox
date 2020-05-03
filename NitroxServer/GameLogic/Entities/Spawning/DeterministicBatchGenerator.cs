using System;
using DTO = NitroxModel.DataStructures;

namespace NitroxServer.GameLogic.Entities.Spawning
{
    public class DeterministicBatchGenerator
    {
        private readonly Random random;

        public DeterministicBatchGenerator(DTO.Int3 batchId)
        {
            random = new Random(batchId.GetHashCode());
        }

        public double NextDouble()
        {
            return random.NextDouble();
        }

        public int NextInt(int min, int max)
        {
            return random.Next(min, max);
        }

        public DTO.NitroxId NextId()
        {
            byte[] bytes = new byte[16];
            random.NextBytes(bytes);

            return new DTO.NitroxId(bytes);
        }
    }
}
