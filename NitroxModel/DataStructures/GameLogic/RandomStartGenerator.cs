using System;
using System.Collections.Generic;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.DataStructures.GameLogic;

public class RandomStartGenerator
{
    private readonly IPixelProvider pixelProvider;

    public RandomStartGenerator(IPixelProvider pixelProvider)
    {
        this.pixelProvider = pixelProvider;
    }

    public NitroxVector3 GenerateRandomStartPosition(Random rnd)
    {
        for (int i = 0; i < 1000; i++)
        {
            float normalizedX = (float)rnd.NextDouble();
            float normalizedZ = (float)rnd.NextDouble();

            if (IsStartPointValid(normalizedX, normalizedZ))
            {
                float x = 4096f * normalizedX - 2048f; // normalizedX = (x + 2048) / 4096
                float z = 4096f * normalizedZ - 2048f;
                return new NitroxVector3(x, 0, z);
            }
        }

        return NitroxVector3.Zero;
    }

    public List<NitroxVector3> GenerateRandomStartPositions(string seed)
    {
        Random rnd = new(seed.GetHashCode());
        List<NitroxVector3> list = new();

        for (int i = 0; i < 1000; i++)
        {
            float normalizedX = (float)rnd.NextDouble();
            float normalizedZ = (float)rnd.NextDouble();

            if (IsStartPointValid(normalizedX, normalizedZ))
            {
                float x = 4096f * normalizedX - 2048f; // normalizedX = (x + 2048) / 4096
                float z = 4096f * normalizedZ - 2048f;
                list.Add(new NitroxVector3(x, 0, z));
            }
        }

        return list;
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
