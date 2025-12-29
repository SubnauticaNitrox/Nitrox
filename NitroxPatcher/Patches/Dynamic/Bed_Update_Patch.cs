using System.Reflection;
using NitroxClient.GameLogic;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Replaces the bed exit behavior when sleeping to use our SleepManager.
/// Uses Prefix instead of Transpiler because we completely replace the sleeping branch logic
/// rather than modifying individual lines.
/// </summary>
public sealed partial class Bed_Update_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Bed t) => t.Update());

    public static bool Prefix(Bed __instance)
    {
        // Let original code run for non-sleeping states
        if (__instance.inUseMode != Bed.InUseMode.Sleeping)
        {
            return true;
        }

        SleepManager sleepManager = Resolve<SleepManager>();
        if (sleepManager.CanExitBed)
        {
            HandReticle.main.SetText(HandReticle.TextType.Hand, "Get Up", true, GameInput.Button.Exit);
            HandReticle.main.SetIcon(HandReticle.IconType.None, 1f);

            if (GameInput.GetButtonDown(GameInput.Button.Exit))
            {
                sleepManager.ExitBed();
            }
        }

        return false;
    }
}
