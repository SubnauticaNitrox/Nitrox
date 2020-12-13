using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using NitroxModel.DataStructures.GameLogic;

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
                rnd = new Random(Environment.TickCount);
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
                    float x = normalizedX * 4096f - 2048f;
                    float z = normalizedZ * 4096f - 2048f;
                    return new NitroxVector3(x, 0, z);
                }
            }

            return NitroxVector3.Zero;
        }

        private bool IsStartPointValid(float normalizedX, float normalizedZ)
        {
            int textureX = (int)(normalizedX * randomStartTexture.Width);
            int textureZ = (int)(normalizedZ * randomStartTexture.Height);

            Color pixelColor = randomStartTexture.GetPixel(textureX, textureZ);

            return pixelColor.G > 126;
        }
    }
}
