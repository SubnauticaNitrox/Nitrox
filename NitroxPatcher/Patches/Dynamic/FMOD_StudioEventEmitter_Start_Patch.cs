using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.Logger;

namespace NitroxPatcher.Patches.Dynamic
{
    public class FMOD_StudioEventEmitter_Start_Patch : NitroxPatch, IDynamicPatch
    {
        private static FMODSystem fmodSystem;

        private static readonly MethodInfo targetMethod = typeof(FMOD_StudioEventEmitter).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(FMOD_StudioEventEmitter __instance)
        {
            if (fmodSystem.IsWhitelisted(__instance.asset.path, out bool _, out float radius))
            {
                if (__instance.TryGetComponent(out NitroxEntity nitroxEntity))
                {
                    nitroxEntity.gameObject.AddComponent<FMODEmitterController>().AddEmitter(__instance.asset.path, __instance, radius);
                }
                else
                {
                    NitroxEntity nitroxParentEntity = __instance.GetComponentInParent<NitroxEntity>();
                    if (nitroxParentEntity)
                    {
                        nitroxParentEntity.gameObject.AddComponent<FMODEmitterController>().AddEmitter(__instance.asset.path, __instance, radius);
                    }
                    else
                    {
                        Log.Warn($"[FMOD_CustomEmitter_Start_Patch] - No NitroxEntity for \"{__instance.asset.path}\" found!");
                    }
                }
            }
        }

        public override void Patch(Harmony harmony)
        {
            fmodSystem = NitroxServiceLocator.LocateService<FMODSystem>();
            PatchPostfix(harmony, targetMethod);
        }
    }
}
