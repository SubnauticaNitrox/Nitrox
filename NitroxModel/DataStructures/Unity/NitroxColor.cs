using System;
using System.Runtime.Serialization;
using NitroxModel.Helper;

namespace NitroxModel.DataStructures.Unity
{
    [DataContract]
    [Serializable]
    public struct NitroxColor
    {
        [DataMember(Order = 1)]
        public float R { get; private set; }

        [DataMember(Order = 2)]
        public float G { get; private set; }

        [DataMember(Order = 3)]
        public float B { get; private set; }

        [DataMember(Order = 4)]
        public float A { get; private set; }

        public NitroxColor(float r, float g, float b, float a = 1)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public static NitroxColor FromHsb(float hue, float saturation = 1, float brightness = 1)
        {
            hue = Mathf.Clamp01(hue);
            saturation = Mathf.Clamp01(saturation);
            brightness = Mathf.Clamp01(brightness);

            hue *= 360f;
            float r = 0;
            float g = 0;
            float b = 0;

            if (saturation == 0)
            {
                r = g = b = brightness;
            }
            else
            {
                // the color wheel consists of 6 sectors.
                float sectorPos = hue / 60.0f;
                int sectorNumber = (int)Math.Floor(sectorPos);
                // get the fractional part of the sector
                float fractionalSector = sectorPos - sectorNumber;

                // calculate values for the three axes of the color.
                float p = brightness * (1f - saturation);
                float q = brightness * (1f - saturation * fractionalSector);
                float t = brightness * (1f - saturation * (1f - fractionalSector));

                // assign the fractional colors to r, g, and b based on the sector the angle is in.
                switch (sectorNumber)
                {
                    case 0:
                        r = brightness;
                        g = t;
                        b = p;
                        break;
                    case 1:
                        r = q;
                        g = brightness;
                        b = p;
                        break;
                    case 2:
                        r = p;
                        g = brightness;
                        b = t;
                        break;
                    case 3:
                        r = p;
                        g = q;
                        b = brightness;
                        break;
                    case 4:
                        r = t;
                        g = p;
                        b = brightness;
                        break;
                    case 5:
                        r = brightness;
                        g = p;
                        b = q;
                        break;
                }
            }

            if (r < 0)
            {
                r = 0;
            }
            if (g < 0)
            {
                g = 0;
            }
            if (b < 0)
            {
                b = 0;
            }
            return new NitroxColor(r, g, b);
        }

        public override string ToString()
        {
            return $"[NitroxColor: {R}, {G}, {B}, {A}]";
        }
    }
}
