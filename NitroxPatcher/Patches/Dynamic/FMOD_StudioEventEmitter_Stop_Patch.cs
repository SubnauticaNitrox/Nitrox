using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class FMOD_StudioEventEmitter_Stop_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((FMOD_StudioEventEmitter t) => t.Stop(default(bool)));

        public static void Postfix(FMOD_StudioEventEmitter __instance, bool allowFadeout)
        {
            if (Resolve<FMODSystem>().IsWhitelisted(__instance.asset.path))
            {
                __instance.TryGetComponent(out NitroxEntity nitroxEntity);
                if (!nitroxEntity)
                {
                    nitroxEntity = __instance.GetComponentInParent<NitroxEntity>();
                }
                if (nitroxEntity)
                {
                    Resolve<FMODSystem>().PlayStudioEmitter(nitroxEntity.Id, __instance.asset.path, false, allowFadeout);
                }
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
