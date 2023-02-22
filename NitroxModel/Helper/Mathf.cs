using System;

namespace NitroxModel.Helper
{
    public class Mathf
    {
        public const float RAD2DEG = 57.29578f;
        public const float PI = 3.14159274f;
        public const float DEG2RAD = 0.0174532924f;

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

        public static float Pow(float p1, float p2)
        {
            return (float)Math.Pow(p1, p2);
        }

        /// <summary>
        ///     Clamps the given value between 0 and 1.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float Clamp01(float value)
        {
            // Not using Clamp as an optimization.
            if (value < 0)
            {
                return 0;
            }
            if (value > 1)
            {
                return 1;
            }
            return value;
        }

        public static T Clamp<T>(T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0)
            {
                return min;
            }
            if (val.CompareTo(max) > 0)
            {
                return max;
            }
            return val;
        }

        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="digits" /> is less than 0 or greater than 15.</exception>
        public static float Round(float value, int digits = 0)
        {
            return (float)Math.Round(value, digits);
        }

        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }
    }
}
