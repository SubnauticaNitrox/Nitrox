using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class FMOD_StudioEventEmitter_PlayUI_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((FMOD_StudioEventEmitter t) => t.PlayUI());

        public static void Postfix(FMOD_StudioEventEmitter __instance, float ____lastTimePlayed)
        {
            if (Resolve<FMODSystem>().IsWhitelisted(__instance.asset.path))
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
                        Resolve<FMODSystem>().PlayStudioEmitter(nitroxEntity.Id, __instance.asset.path, true, false);
                    }
                }
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
