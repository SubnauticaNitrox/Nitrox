using UnityEngine;

namespace NitroxClient.Unity.Smoothing
{
    public class SmoothRotation
    {
        private const float SMOOTHING_SPEED = 10f;
        public Quaternion Target { get; set; }
        public Quaternion Current { get; set; }

        public SmoothRotation(Quaternion initial)
        {
            Target = Current = initial;
        }

        public SmoothRotation()
        {
        }

        public void FixedUpdate()
        {
            Current = Quaternion.Slerp(Current, Target, SMOOTHING_SPEED * Time.fixedDeltaTime);
        }
    }
}
