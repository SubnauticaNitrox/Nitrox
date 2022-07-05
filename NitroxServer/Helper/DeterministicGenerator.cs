using System;
using NitroxModel.DataStructures;

namespace NitroxServer.Helper
{
    public class DeterministicGenerator
    {
        private readonly Random random;

        public DeterministicGenerator(string worldSeed, object reference)
        {
            random = new Random(worldSeed.GetHashCode() + reference.GetHashCode());
        }

        public double NextDouble()
        {
            return random.NextDouble();
        }

        public double NextDouble(double min, double max)
        {
            return random.NextDouble() * (max - min) + min;
        }

        public int NextInt(int min, int max)
        {
            return random.Next(min, max);
        }

        public NitroxId NextId()
        {
            byte[] bytes = new byte[16];
            random.NextBytes(bytes);
            return new NitroxId(bytes);
        }
    }
}
