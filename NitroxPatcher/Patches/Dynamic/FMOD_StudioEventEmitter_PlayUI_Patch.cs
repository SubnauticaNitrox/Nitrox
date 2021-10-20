using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class FMOD_StudioEventEmitter_PlayUI_Patch : NitroxPatch, IDynamicPatch
    {
        private static FMODSystem fmodSystem;

        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((FMOD_StudioEventEmitter t) => t.PlayUI());

        public static bool Prefix()
        {
            return !FMODSuppressor.SuppressFMODEvents;
        }

        public static void Postfix(FMOD_StudioEventEmitter __instance, float ____lastTimePlayed)
        {
            if (fmodSystem.IsWhitelisted(__instance.asset.path))
            {
                if (____lastTimePlayed == 0.0 || Time.time > ____lastTimePlayed + __instance.minInterval)
                {
                    __instance.TryGetComponent(out NitroxEntity nitroxEntity);
                    if (!nitroxEntity)
                    {
                        nitroxEntity = __instance.GetComponentInParent<NitroxEntity>();
                    }
                    if (nitroxEntity)
                    {
                        fmodSystem.PlayStudioEmitter(nitroxEntity.Id, __instance.asset.path, true, false);
                    }
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
