using UnityEngine;

namespace NitroxClient.Unity.Helper
{
    public class MathUtil
    {
        public static Vector3 ClampMagnitude(Vector3 v, float max, float min)
        {
            double sm = v.sqrMagnitude;
            if (sm > (double)max * max)
            {
                return v.normalized * max;
            }
            else if (sm < (double)min * min)
            {
                return v.normalized * min;
            }

            return v;
        }
    }
}
