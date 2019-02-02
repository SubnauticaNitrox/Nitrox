﻿using System;

namespace NitroxServer.GameLogic.Entities.Spawning
{
    public class DeterministicBatchGenerator
    {
        private static readonly string prefix = "nitrox-";

        private Random random;

        public DeterministicBatchGenerator(Int3 batchId)
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

        public string NextGuid()
        {
            byte[] bytes = new byte[16];
            random.NextBytes(bytes);

            return prefix + Convert.ToBase64String(bytes);
        }
    }
}
