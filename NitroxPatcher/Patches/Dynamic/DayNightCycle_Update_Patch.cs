using System.Reflection;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Replace the local time calculations by the real server time.
/// </summary>
public sealed partial class DayNightCycle_Update_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((DayNightCycle t) => t.Update());

    public static bool Prefix(DayNightCycle __instance)
    {
        __instance.timePassedAsDouble = Resolve<TimeManager>().CalculateCurrentTime();

        if (__instance.IsInSkipTimeMode() && __instance.timePassed >= __instance.skipModeEndTime)
        {
            __instance.StopSkipTimeMode();
        }

        __instance.UpdateAtmosphere();
        __instance.UpdateDayNightMessage();
        return false;
    }
}
