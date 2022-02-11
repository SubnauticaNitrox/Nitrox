using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class FMOD_CustomLoopingEmitter_OnPlay_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((FMOD_CustomLoopingEmitter t) => t.OnPlay());

        public static void Postfix(FMOD_CustomLoopingEmitter __instance)
        {
            if (__instance.assetStart && Resolve<FMODSystem>().IsWhitelisted(__instance.assetStart.path))
            {
                __instance.TryGetComponent(out NitroxEntity nitroxEntity);
                if (!nitroxEntity)
                {
                    nitroxEntity = __instance.GetComponentInParent<NitroxEntity>();
                }
                if (nitroxEntity)
                {
                    Resolve<FMODSystem>().PlayCustomLoopingEmitter(nitroxEntity.Id, __instance.assetStart.path);
                }
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
