using UnityEngine;

namespace NitroxClient.Unity.Smoothing
{
    class ExosuitSmoothRotation : SmoothRotation
    {
        private const float SMOOTHING_SPEED = 20f;
        private float timeCount = 0;
        private Quaternion target;
        public new Quaternion Target
        {
            get { return target; }
            set
            {
                timeCount = 0;
                target = value;
            }
        }

        public ExosuitSmoothRotation(Quaternion initial)
        {
            Target = Current = initial;
        }

        public ExosuitSmoothRotation()
        {
        }

        public new void FixedUpdate()
        {
            Current = Quaternion.Slerp(Current, Target, timeCount + SMOOTHING_SPEED * Time.fixedDeltaTime);
            timeCount += SMOOTHING_SPEED * Time.fixedDeltaTime;
        }
    }
}
