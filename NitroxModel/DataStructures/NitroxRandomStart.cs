using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;

namespace NitroxModel.DataStructures
{
    public class NitroxRandomStart
    {
        private readonly Bitmap randomStartTexture;

        public NitroxRandomStart(Bitmap randomStartTexture)
        {
            this.randomStartTexture = randomStartTexture;
        }

        public NitroxVector3 GenerateRandomStartPosition(string seed)
        {

            Random rnd;
            if (seed == null || seed.Trim().Length == 0)
            {
                rnd = new Random(StringHelper.GenerateRandomString(10).GetHashCode());
            }
            else
            {
                rnd = new Random(seed.GetHashCode());
            }

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

        private bool IsStartPointValid(float normalizedX, float normalizedZ)
        {
            int textureX = (int)(normalizedX * (float)512);
            int textureZ = (int)(normalizedZ * (float)512);

            Color pixelColor = randomStartTexture.GetPixel(textureX, textureZ);

            return pixelColor.G > 127;
        }
    }
}
