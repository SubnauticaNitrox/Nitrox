namespace NitroxClient.GameLogic.FMOD
{
    public readonly struct SoundData
    {
        public bool IsWhitelisted { get; }
        public bool IsGlobal { get; }
        public float SoundRadius { get; }

        public SoundData(bool isWhitelisted, bool isGlobal, float soundRadius)
        {
            IsWhitelisted = isWhitelisted;
            IsGlobal = isGlobal;
            SoundRadius = soundRadius;
        }
    }
}
