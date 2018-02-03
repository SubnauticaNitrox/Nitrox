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
            TargetValue = targetValue;
            CurrentValue = currentValue;
            MaxValue = maxValue;
            SmoothTime = smoothTime;
        }
    }
}
