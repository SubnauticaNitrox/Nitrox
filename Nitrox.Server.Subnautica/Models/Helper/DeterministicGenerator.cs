using Nitrox.Model.DataStructures;

namespace Nitrox.Server.Subnautica.Models.Helper;

internal sealed class DeterministicGenerator(string worldSeed, string secondSeedValue)
{
    private readonly Random random = new($"{worldSeed}{secondSeedValue}".ToMd5HashedInt32());

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
