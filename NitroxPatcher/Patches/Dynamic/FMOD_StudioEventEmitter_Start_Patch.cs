using System.Reflection;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class FMOD_StudioEventEmitter_Start_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((FMOD_StudioEventEmitter t) => t.Start());

    public static void Postfix(FMOD_StudioEventEmitter __instance)
    {
        if (!Resolve<FMODSystem>().IsWhitelisted(__instance.asset.path, out bool _, out float radius))
        {
            return;
        }

        if (!__instance.TryGetComponentInParent(out NitroxEntity entity, true))
        {
            Log.Warn($"[FMOD_StudioEventEmitter_Start_Patch] - No NitroxEntity found for {__instance.asset.path} at {__instance.GetFullHierarchyPath()}");
            return;
        }

        FMODEmitterController fmodController = entity.gameObject.EnsureComponent<FMODEmitterController>();
        fmodController.AddEmitter(__instance.asset.path, __instance, radius);
    }
}
