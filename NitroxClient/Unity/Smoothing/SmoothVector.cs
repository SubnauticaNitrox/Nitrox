using UnityEngine;

namespace NitroxClient.Unity.Smoothing
{
    public class SmoothVector
    {
        private const float SMOOTHING_SPEED = 10f;
        public Vector3 Target { get; set; }
        public Vector3 Current { get; set; }

        public SmoothVector(Vector3 initial)
        {
            Target = Current = initial;
        }

        public SmoothVector()
        {
        }

        public void FixedUpdate()
        {
            Current = UWE.Utils.SlerpVector(Current, Target, (Target - Current).normalized * SMOOTHING_SPEED * Time.fixedDeltaTime);
        }
    }
}
