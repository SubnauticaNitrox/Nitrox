using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    public class FMOD_CustomEmitter_OnStop_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo targetMethod = typeof(FMOD_CustomEmitter).GetMethod("OnStop", BindingFlags.NonPublic | BindingFlags.Instance);

        private static FMODSystem fmodSystem;

        public static void Postfix(FMOD_CustomEmitter __instance)
        {
            if (fmodSystem.IsWhitelisted(__instance.asset.path))
            {
                if (__instance.TryGetComponent(out NitroxEntity nitroxEntity))
                {
                    int componentId = Array.IndexOf(__instance.GetComponents<FMOD_CustomEmitter>(), __instance);
                    fmodSystem.PlayFMOD_CustomEmitter(nitroxEntity.Id, componentId, false);
                }
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            fmodSystem = NitroxServiceLocator.LocateService<FMODSystem>();
            PatchPostfix(harmony, targetMethod);
        }

    }
}
