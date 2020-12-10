using System.Reflection;
using Harmony;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class FMOD_StudioEventEmitter_PlayUI_Patch : NitroxPatch, IDynamicPatch
    {
        private static FMODSystem fmodSystem;

        private static readonly MethodInfo targetMethod = typeof(FMOD_StudioEventEmitter).GetMethod(nameof(FMOD_StudioEventEmitter.PlayUI), BindingFlags.Public | BindingFlags.Instance);

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

        public override void Patch(HarmonyInstance harmony)
        {
            fmodSystem = NitroxServiceLocator.LocateService<FMODSystem>();
            PatchMultiple(harmony, targetMethod, true, true, false);
        }

    }
}
