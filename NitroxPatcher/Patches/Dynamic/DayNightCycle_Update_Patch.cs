using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Replace the local time calculations by the real server time.
/// </summary>
public sealed partial class DayNightCycle_Update_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((DayNightCycle t) => t.Update());

    public static bool Prefix(DayNightCycle __instance)
    {
        // Essential part of the Update() method to have it running all of the time and have the local time set to the real server time
        __instance.timePassedAsDouble = Resolve<TimeManager>().CalculateCurrentTime();
        __instance.UpdateAtmosphere();
        __instance.UpdateDayNightMessage();
        return false;
    }
}
