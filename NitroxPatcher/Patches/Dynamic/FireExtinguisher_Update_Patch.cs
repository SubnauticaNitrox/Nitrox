using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public class FireExtinguisher_Update_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((FireExtinguisher t) => t.Update());

    /// <summary>
    /// Cancel the Update() method of remote players' FireExtinguishers, and move their code to another place
    /// </summary>
    public static bool Prefix(FireExtinguisher __instance)
    {
        // Only for remote players' fire extinguishers
        if (__instance.transform.TryGetComponentInAscendance(12, out RemotePlayerIdentifier identifier))
        {
            Resolve<FireExtinguisherManager>().RemotelyUseFireExtinguisher(__instance, identifier);
            return false;
        }
        return true;
    }

    /// <summary>
    /// Notice other players that LocalPlyaer is starting/stopping to use a fire extinguisher
    /// </summary>
    public static void Postfix(FireExtinguisher __instance)
    {
        if (!__instance.transform.TryGetComponentInAscendance(2, out RemotePlayerIdentifier _) &&
            NitroxEntity.TryGetEntityFrom(__instance.gameObject, out NitroxEntity entity))
        {
            Resolve<FireExtinguisherManager>().BroadcastFireExtinguisherState(entity.Id, __instance);
        }
    }

    public override void Patch(Harmony harmony)
    {
        PatchMultiple(harmony, TARGET_METHOD, prefix: true, postfix: true);
    }
}
