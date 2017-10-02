using UnityEngine;

namespace NitroxModel.Helper.Math
{
    public class SmoothRotation
    {
        private const float SMOOTHING_SPEED = 10f;
        public Quaternion Target { get; set; }
        public Quaternion SmoothValue { get; private set; }

        public SmoothRotation(Quaternion initial)
        {
            Target = SmoothValue = initial;
        }

        public SmoothRotation()
        {
        }

        public void FixedUpdate()
        {
            SmoothValue = Quaternion.Slerp(SmoothValue, Target, SMOOTHING_SPEED * Time.fixedDeltaTime);
        }
    }
}
