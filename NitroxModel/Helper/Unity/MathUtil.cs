using UnityEngine;

namespace NitroxModel.Helper.Unity
{
    public class MathUtil
    {
        public static Vector3 ClampMagnitude(Vector3 v, float max, float min)
        {
            double sm = v.sqrMagnitude;
            if (sm > (double)max * (double)max) return v.normalized * max;
            else if (sm < (double)min * (double)min) return v.normalized * min;
            return v;
        }
    }
}
