using System.Reflection;
using FMOD.Studio;
using HarmonyLib;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.Logger;

namespace NitroxPatcher.Patches.Dynamic
{
    public class FMOD_CustomEmitter_Start_Patch : NitroxPatch, IDynamicPatch
    {
        private static FMODSystem fmodSystem;

        private static readonly MethodInfo targetMethod = typeof(FMOD_CustomEmitter).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Prefix(FMOD_CustomEmitter __instance)
        {
            if (fmodSystem.IsWhitelisted(__instance.asset.path, out bool _, out float radius))
            {
                __instance.GetEventInstance().getDescription(out EventDescription description);
                description.is3D(out bool is3D);

                if (__instance.TryGetComponent(out NitroxEntity nitroxEntity) && is3D)
                {
                    nitroxEntity.gameObject.AddComponent<FMODEmitterController>().AddEmitter(__instance.asset.path, __instance, radius);
                }
                else if (is3D)
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
            PatchPrefix(harmony, targetMethod);
        }
    }
}
