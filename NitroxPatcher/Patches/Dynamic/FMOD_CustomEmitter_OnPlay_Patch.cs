using System.Reflection;
using FMOD.Studio;
using HarmonyLib;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.Helper;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxPatcher.Patches.Dynamic
{
    public class FMOD_CustomEmitter_OnPlay_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((FMOD_CustomEmitter t) => t.OnPlay());

        public static bool Prefix()
        {
            return !FMODSuppressor.SuppressFMODEvents;
        }

        public static void Postfix(FMOD_CustomEmitter __instance)
        {
            if (Resolve<FMODSystem>().IsWhitelisted(__instance.asset.path, out bool isGlobal, out float radius))
            {
                __instance.GetEventInstance().getDescription(out EventDescription description);
                description.is3D(out bool is3D);

                if (is3D)
                {
                    __instance.TryGetComponent(out NitroxEntity nitroxEntity);
                    if (!nitroxEntity)
                    {
                        nitroxEntity = __instance.GetComponentInParent<NitroxEntity>();
                    }
                    if (nitroxEntity)
                    {
                        Resolve<FMODSystem>().PlayCustomEmitter(nitroxEntity.Id, __instance.asset.path, true);
                    }
                }
                else
                {
                    __instance.GetEventInstance().getVolume(out float volume, out float _);
                    Resolve<FMODSystem>().PlayAsset(__instance.asset.path, __instance.transform.position.ToDto(), volume, radius, isGlobal);
                }
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, prefix:true, postfix:true);
        }

    }
}
