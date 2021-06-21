using System;

namespace NitroxModel.Helper
{
    public class Mathf
    {
        public const float RAD2DEG = 57.29578f;
        public const float PI = 3.14159274f;
        public const float DEG2RAD = 0.0174532924f;

        public static float Cos(float cs)
        {
            return (float)Math.Cos(cs);
        }

        public static float Sin(float sn)
        {
            return (float)Math.Sin(sn);
        }

        public static float Sqrt(float ls)
        {
            return (float)Math.Sqrt(ls);
        }

        public static float Atan2(float p1, float p2)
        {
            return (float)Math.Atan2(p1, p2);
        }

        public static float Asin(float p)
        {
            return (float)Math.Asin(p);
        }

        // Copied from Unity
        public static float Max(params float[] values)
        {
            int length = values.Length;
            if (length == 0)
            {
                return 0.0f;
            }

            float num = values[0];
            for (int index = 1; index < length; ++index)
            {
                if ((double) values[index] > (double) num)
                {
                    num = values[index];
                }
            }
            return num;
        }

        // Copied from Unity
        public static float Sign(float f) => f < 0.0 ? -1f : 1f;
      
        public static float Pow(float p1, float p2)
        {
            return (float)Math.Pow(p1, p2);
        }

        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="digits" /> is less than 0 or greater than 15.</exception>
        public static float Round(float value, int digits = 0)
        {
            return (float)Math.Round(value, digits);
        }
    }
}
