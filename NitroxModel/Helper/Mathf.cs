using System;

namespace NitroxModel.Helper
{
    public static class Mathf
    {
        public const float RAD_TO_DEG = 57.29578f;
        public const float PI = 3.14159274f;
        public const float DEG_TO_RAD = 0.0174532924f;

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
    }
}
