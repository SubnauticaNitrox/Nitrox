using System.Reflection;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.Overrides;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class PlayerCinematicController_Start_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo targetMethod = Reflect.Method((PlayerCinematicController t) => t.Start());

    public static void Postfix(PlayerCinematicController __instance)
    {
        if (!__instance.TryGetComponentInParent(out NitroxEntity entity, true))
        {
            Log.Warn($"[PlayerCinematicController_Start_Patch] - No NitroxEntity for \"{__instance.GetFullHierarchyPath()}\" found!");
            return;
        }

        if (!entity.gameObject.TryGetComponent(out MultiplayerCinematicReference reference))
        {
            reference = entity.gameObject.AddComponent<MultiplayerCinematicReference>();
        }

        reference.AddController(__instance);
    }
}
