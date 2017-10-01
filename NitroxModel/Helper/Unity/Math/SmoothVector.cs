using UnityEngine;

namespace NitroxModel.Helper.Math
{
    public class SmoothVector
    {
        private const float SMOOTHING_SPEED = 10f;
        public Vector3 Target { get; set; }
        public Vector3 SmoothValue { get; private set; }

        public SmoothVector(Vector3 initial)
        {
            Target = SmoothValue = initial;
        }

        public SmoothVector()
        {
        }

        public void FixedUpdate()
        {
            SmoothValue = UWE.Utils.SlerpVector(SmoothValue, Target, (Target - SmoothValue).normalized * SMOOTHING_SPEED * Time.fixedDeltaTime);
        }
    }
}
