using NitroxModel.Helper;

namespace NitroxModel.GameLogic.FMOD;

public static class SoundHelper
{
    /// <summary>
    ///     Volume calculation based on <paramref name="distance"/>, <paramref name="radius"/> and max <paramref name="volume"/> (not 100% realistic)
    /// </summary>
    public static float CalculateVolume(float distance, float radius, float volume)
    {
        if (distance >= 0f && distance < radius)
        {
            return Mathf.Clamp01((1f - distance / radius) * volume);
        }
        return 0f;
    }
}
