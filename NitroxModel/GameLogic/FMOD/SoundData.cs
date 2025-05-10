namespace NitroxModel.GameLogic.FMOD;

public record SoundData
{
    public bool IsWhitelisted { get; }
    public bool IsGlobal { get; }
    public float Radius { get; }

    public SoundData(bool isWhitelisted, bool isGlobal, float radius)
    {
        IsWhitelisted = isWhitelisted;
        IsGlobal = isGlobal;
        Radius = radius;
    }
}
