using System;
using System.Collections.Generic;
using Nitrox.Model.DataStructures.Unity;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic;

public sealed class RandomStartGenerator(RandomStartGenerator.IPixelProvider pixelProvider)
{
    private readonly IPixelProvider pixelProvider = pixelProvider;

    /// <summary>
    ///     Generates all starts positions available for a given randomization. Only take as many positions as needed to avoid unnecessary compute.
    /// </summary>
    public IEnumerable<NitroxVector3> GenerateAllStartPositions(Random random)
    {
        for (int i = 0; i < int.MaxValue; i++)
        {
            float normalizedX = (float)random.NextDouble();
            float normalizedZ = (float)random.NextDouble();

            if (IsStartPointValid(normalizedX, normalizedZ))
            {
                float x = 4096f * normalizedX - 2048f; // normalizedX = (x + 2048) / 4096
                float z = 4096f * normalizedZ - 2048f;
                yield return new NitroxVector3(x, 0, z);
            }
        }
    }

    private bool IsStartPointValid(float normalizedX, float normalizedZ)
    {
        int textureX = (int)(normalizedX * 512);
        int textureZ = (int)(normalizedZ * 512);

        return pixelProvider.GetGreen(textureX, textureZ) > 127;
    }

    /// <summary>
    ///     API for getting pixels from an underlying texture.
    /// </summary>
    public interface IPixelProvider
    {
        byte GetRed(int x, int y);
        byte GetGreen(int x, int y);
        byte GetBlue(int x, int y);
    }
}
