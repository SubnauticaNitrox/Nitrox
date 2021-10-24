using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class FMOD_CustomLoopingEmitter_PlayStopSound_Patch : NitroxPatch, IDynamicPatch
    {
        private static FMODSystem fmodSystem;

        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((FMOD_CustomLoopingEmitter t) => t.PlayStopSound());

        public static bool Prefix()
        {
            return !FMODSuppressor.SuppressFMODEvents;
        }

        public static void Postfix(FMOD_CustomLoopingEmitter __instance)
        {
            if (__instance.assetStop && fmodSystem.IsWhitelisted(__instance.assetStop.path))
            {
                __instance.TryGetComponent(out NitroxEntity nitroxEntity);
                if (!nitroxEntity)
                {
                    nitroxEntity = __instance.GetComponentInParent<NitroxEntity>();
                }
                if (nitroxEntity)
                {
                    fmodSystem.PlayCustomLoopingEmitter(nitroxEntity.Id, __instance.assetStop.path);
                }
            }
        }

        public override void Patch(Harmony harmony)
        {
            fmodSystem = NitroxServiceLocator.LocateService<FMODSystem>();
            PatchMultiple(harmony, TARGET_METHOD, prefix:true, postfix:true);
        }
    }
}
