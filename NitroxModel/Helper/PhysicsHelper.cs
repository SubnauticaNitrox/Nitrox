namespace NitroxModel.Helper;

public class PhysicsHelper
{
    /// <summary>
    ///     Non realistic volume calculation but enough for us
    /// </summary>
    public static float CalculateVolume(float distance, float radius, float volume)
    {
        return (1 - distance / radius) * volume;
    }
}
