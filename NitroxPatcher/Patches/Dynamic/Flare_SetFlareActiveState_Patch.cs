using System.Reflection;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Sets <see cref="Flare.flareActivateTime"/> using <see cref="TimeManager.RealTimeElapsed"/>
/// instead of <see cref="DayNightCycle.main.timePassedAsFloat"/> to make flares immune to time skips
/// </summary>
public sealed partial class Flare_SetFlareActiveState_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Flare t) => t.SetFlareActiveState(default));

    public static void Postfix(Flare __instance, bool newFlareActiveState)
    {
        if (newFlareActiveState && __instance.flareActiveState)
        {
            __instance.flareActivateTime = (float)Resolve<TimeManager>().RealTimeElapsed;
        }
    }
}
