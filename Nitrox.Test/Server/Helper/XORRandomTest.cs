using Nitrox.Server.Subnautica.Models.Helper;

namespace Nitrox.Test.Server.Helper;

[TestClass]
public class XORRandomTest
{
    [TestMethod]
    public void TestMeanGeneration()
    {
        // arbitrary values under there but we can't compare the generated values with UnityEngine.Random because it's unaccessible
        XorRandom.InitSeed("cheescake".GetHashCode());
        float mean = 0;
        int count = 1000000;
        for (int i = 0; i < count; i++)
        {
            mean += XorRandom.NextFloat();
        }
        mean /= count;
        Assert.IsTrue(Math.Abs(0.5f - mean) < 0.001f, $"Float number generation isn't uniform enough: {mean}");
    }
}
