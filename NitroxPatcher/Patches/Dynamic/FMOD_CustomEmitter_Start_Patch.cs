using System.Reflection;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class FMOD_CustomEmitter_Start_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((FMOD_CustomEmitter t) => t.Start());

    public static void Prefix(FMOD_CustomEmitter __instance)
    {
        if (!Resolve<FMODSystem>().IsWhitelisted(__instance.asset.path, out float radius))
        {
            return;
        }

        if (!__instance.TryGetComponentInParent(out NitroxEntity entity, true))
        {
            if (__instance.GetRootParent().gameObject.name != "__LIGHTMAPPED_PREFAB__") // ignore calls from "blueprint prefabs"
            {
                Log.Warn($"[FMOD_CustomEmitter_Start_Patch] - No NitroxEntity found for {__instance.asset.path} at {__instance.gameObject.GetFullHierarchyPath()}");
            }
            return;
        }

        FMODEmitterController fmodController = entity.gameObject.EnsureComponent<FMODEmitterController>();
        fmodController.AddEmitter(__instance.asset.path, __instance, radius);
    }
}
