using System.Reflection;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.CinematicController;
using NitroxClient.Unity.Helper;
using NitroxModel_Subnautica.Helper;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class PlayerCinematicController_Start_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo targetMethod = Reflect.Method((PlayerCinematicController t) => t.Start());

    public static void Postfix(PlayerCinematicController __instance)
    {
        if (!__instance.TryGetComponentInParent(out NitroxEntity entity, true))
        {
            if (__instance.GetRootParent().gameObject.name != SubnauticaConstants.LIGHTMAPPED_PREFAB_NAME) // ignore calls from "blueprint prefabs"
            {
                Log.Warn($"[PlayerCinematicController_Start_Patch] - No NitroxEntity for \"{__instance.gameObject.GetFullHierarchyPath()}\" found!");
            }

            return;
        }

        entity.gameObject.EnsureComponent<MultiplayerCinematicReference>().AddController(__instance);
    }
}
