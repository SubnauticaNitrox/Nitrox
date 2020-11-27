using System;
using System.Reflection;
using FMOD.Studio;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxPatcher.Patches.Dynamic
{
    public class FMOD_CustomEmitter_OnPlay_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo targetMethod = typeof(FMOD_CustomEmitter).GetMethod("OnPlay", BindingFlags.NonPublic | BindingFlags.Instance);

        private static FMODSystem fmodSystem;

        public static void Postfix(FMOD_CustomEmitter __instance)
        {
            if (fmodSystem.IsWhitelisted(__instance.asset.path, out bool isGlobal))
            {
                __instance.GetEventInstance().getDescription(out EventDescription description);
                description.getMaximumDistance(out float maxDistance);
                description.is3D(out bool is3D);

                if (__instance.TryGetComponent(out NitroxEntity nitroxEntity) && is3D)
                {
                    int componentId = Array.IndexOf(__instance.GetComponents<FMOD_CustomEmitter>(), __instance);
                    fmodSystem.PlayFMOD_CustomEmitter(nitroxEntity.Id, componentId, true);
                }
                else
                {
                    float radius = maxDistance <= -1 ? 20f : maxDistance;
                    fmodSystem.PlayFMODAsset(__instance.asset.path, __instance.transform.position.ToDto(), radius, isGlobal);
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
