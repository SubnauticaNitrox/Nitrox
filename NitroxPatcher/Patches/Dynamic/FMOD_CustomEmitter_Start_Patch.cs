using System.Reflection;
using FMOD.Studio;
using HarmonyLib;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class FMOD_CustomEmitter_Start_Patch : NitroxPatch, IDynamicPatch
    {
        private static FMODSystem fmodSystem;

        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((FMOD_CustomEmitter t) => t.Start());

        public static void Prefix(FMOD_CustomEmitter __instance)
        {
            if (!fmodSystem.IsWhitelisted(__instance.asset.path, out bool _, out float radius))
            {
                return;
            }

            if (!__instance.TryGetComponent(out NitroxEntity entity))
            {
                entity = __instance.GetComponentInParent<NitroxEntity>();
                if (!entity)
                {
                    Log.Warn($"[FMOD_CustomEmitter_Start_Patch] - No NitroxEntity for \"{__instance.asset.path}\" found!");
                    return;
                }
            }

            if (!entity.gameObject.TryGetComponent(out FMODEmitterController fmodController))
            {
                fmodController = entity.gameObject.AddComponent<FMODEmitterController>();
            }

            EventInstance evt = __instance.GetEventInstance();
            evt.getDescription(out EventDescription description);
            description.is3D(out bool is3D);

            if (is3D)
            {
                fmodController.AddEmitter(__instance.asset.path, __instance, radius);
            }
            else
            {
                fmodController.AddEventInstance(__instance.asset.path, evt, entity.Id);
            }

            //FMOD_CustomLoopingEmitter has no Start() so we need to check it here
            if (__instance is FMOD_CustomLoopingEmitter looping)
            {
                if (looping.assetStart && fmodSystem.IsWhitelisted(looping.assetStart.path, out bool _, out float radiusStart))
                {
                    fmodController.AddEmitter(looping.assetStart.path, looping, radiusStart);
                }
                if (looping.assetStop && fmodSystem.IsWhitelisted(looping.assetStop.path, out bool _, out float radiusStop))
                {
                    fmodController.AddEmitter(looping.assetStop.path, looping, radiusStop);
                }
            }
        }

        public override void Patch(Harmony harmony)
        {
            fmodSystem = NitroxServiceLocator.LocateService<FMODSystem>();
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
