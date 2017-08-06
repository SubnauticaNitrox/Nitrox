using System;

namespace NitroxClient.MonoBehaviours.Gui.Helper
{
    class SmoothedValue
    {
        public float TargetValue;
        public float CurrentValue;
        public float MaxValue;
        public float SmoothTime;

        public SmoothedValue(float targetValue, float currentValue, float maxValue, float smoothTime)
        {
            this.TargetValue = targetValue;
            this.CurrentValue = currentValue;
            this.MaxValue = maxValue;
            this.SmoothTime = smoothTime;
        }
    }
}
