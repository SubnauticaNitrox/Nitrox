namespace NitroxModel.Helper;

public static class SoundHelper
{
    /// <summary>
    ///     Non realistic volume calculation but enough for us
    /// </summary>
    public static float CalculateVolume(float distance, float radius, float volume)
    {
        if (0f <= distance && distance < radius)
        {
            return (1f - distance / radius) * volume;
        }
        return 0f;
    }
}
