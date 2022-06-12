using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel_Subnautica.Packets;

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
        NitroxId rocketId = NitroxEntity.GetId(rocket.gameObject);
        Resolve<IPacketSender>().Send(new RocketLaunch(rocketId));
        return false;
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}
