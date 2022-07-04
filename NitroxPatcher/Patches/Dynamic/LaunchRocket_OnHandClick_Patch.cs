using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public class LaunchRocket_OnHandClick_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((LaunchRocket t) => t.OnHandClick(default));

    public static bool Prefix(LaunchRocket __instance)
    {
        // Copied code from the method itself
        if (!__instance.IsRocketReady() || LaunchRocket.launchStarted)
        {
            return false;
        }
        if (!StoryGoalCustomEventHandler.main.gunDisabled && !__instance.forcedRocketReady)
        {
            __instance.gunNotDisabled.Play();
            return false;
        }
        // Now, instead of launching the rocket, we'll ask the server
        Rocket rocket = __instance.RequireComponentInParent<Rocket>();
        Resolve<Rockets>().RequestRocketLaunch(rocket);
        return false;
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}
