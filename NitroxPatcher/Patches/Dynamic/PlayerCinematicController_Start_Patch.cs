using System.Reflection;
using HarmonyLib;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.Overrides;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public class PlayerCinematicController_Start_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo targetMethod = Reflect.Method((PlayerCinematicController t) => t.Start());

    public static void Postfix(PlayerCinematicController __instance)
    {
        if (!__instance.TryGetComponent(out NitroxEntity entity))
        {
            if (!__instance.TryGetComponentInParent(out entity))
            {
                Log.Warn($"[PlayerCinematicController_Start_Patch] - No NitroxEntity for \"{__instance.GetFullHierarchyPath()}\" found!");
                return;
            }
        }

        if (!entity.gameObject.TryGetComponent(out MultiplayerCinematicReference reference))
        {
            reference = entity.gameObject.AddComponent<MultiplayerCinematicReference>();
        }

        reference.AddController(__instance);
    }

    public override void Patch(Harmony harmony)
    {
        PatchPostfix(harmony, targetMethod);
    }
}
