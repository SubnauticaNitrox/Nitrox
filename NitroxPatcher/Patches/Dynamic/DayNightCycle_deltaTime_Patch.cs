using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Replace the deltaTime calculations by one that is not capped by <see cref="Time.maximumDeltaTime"/>
/// </summary>
public sealed partial class DayNightCycle_deltaTime_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Property((DayNightCycle t) => t.deltaTime).GetGetMethod();

    public static bool Prefix(DayNightCycle __instance, out float __result)
    {
        __result = Resolve<TimeManager>().DeltaTime;
        return false;
    }
}
