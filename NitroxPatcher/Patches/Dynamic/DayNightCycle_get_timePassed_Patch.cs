using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public class DayNightCycle_get_timePassed_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Property((DayNightCycle t) => t.timePassed).GetGetMethod();

    public static bool Prefix(ref double __result)
    {
        if (Resolve<TimeManager>().LatestRegistrationTime == default)
        {
            return true;
        }
        __result = Resolve<TimeManager>().CurrentTime;
        return false;
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}
