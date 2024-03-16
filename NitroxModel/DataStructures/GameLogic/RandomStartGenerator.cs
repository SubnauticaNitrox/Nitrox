using System;
using System.Collections.Generic;
using NitroxModel.DataStructures.Unity;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace NitroxModel.DataStructures.GameLogic;

public class RandomStartGenerator(Image<Bgra32> texture)
{
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
        List<NitroxVector3> list = [];

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
        int textureX = (int)(normalizedX * (float)512);
        int textureZ = (int)(normalizedZ * (float)512);
        Bgra32 pixel = texture[textureX, textureZ];

        return pixel.G > 127;
    }
}
