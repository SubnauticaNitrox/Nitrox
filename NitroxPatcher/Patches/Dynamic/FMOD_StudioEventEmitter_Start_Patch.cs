using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class FMOD_StudioEventEmitter_Start_Patch : NitroxPatch, IDynamicPatch
    {
        private static FMODSystem fmodSystem;

        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((FMOD_StudioEventEmitter t) => t.Start());

        public static void Postfix(FMOD_StudioEventEmitter __instance)
        {
            if (fmodSystem.IsWhitelisted(__instance.asset.path, out bool _, out float radius))
            {
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
                fmodController.AddEmitter(__instance.asset.path, __instance, radius);
            }
        }

        public override void Patch(Harmony harmony)
        {
            fmodSystem = NitroxServiceLocator.LocateService<FMODSystem>();
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
