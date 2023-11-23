using System.Reflection;
using FMOD.Studio;
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
        if (!Resolve<FMODSystem>().IsWhitelisted(__instance.asset.path, out bool _, out float radius))
        {
            return;
        }

        __instance.GetEventInstance().getDescription(out EventDescription description);
        description.is3D(out bool is3D);
        if (!is3D)
        {
            return;
        }

        if (!__instance.TryGetComponentInParent(out NitroxEntity entity))
        {
            Log.Warn($"[FMOD_CustomEmitter_Start_Patch] - No NitroxEntity for \"{__instance.asset.path}\" found!");
            return;
        }

        if (!entity.gameObject.TryGetComponent(out FMODEmitterController fmodController))
        {
            fmodController = entity.gameObject.AddComponent<FMODEmitterController>();
        }
        fmodController.AddEmitter(__instance.asset.path, __instance, radius);

        //FMOD_CustomLoopingEmitter has no Start() so we need to check it here
        if (__instance is FMOD_CustomLoopingEmitter looping)
        {
            if (looping.assetStart && Resolve<FMODSystem>().IsWhitelisted(looping.assetStart.path, out bool _, out float radiusStart))
            {
                fmodController.AddEmitter(looping.assetStart.path, looping, radiusStart);
            }
            if (looping.assetStop && Resolve<FMODSystem>().IsWhitelisted(looping.assetStop.path, out bool _, out float radiusStop))
            {
                fmodController.AddEmitter(looping.assetStop.path, looping, radiusStop);
            }
        }
    }
}
