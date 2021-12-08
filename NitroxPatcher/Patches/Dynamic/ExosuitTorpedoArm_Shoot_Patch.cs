using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.Helper;
using NitroxModel_Subnautica.Packets;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    class ExosuitTorpedoArm_Shoot_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((ExosuitTorpedoArm t) => t.Shoot(default(TorpedoType), default(Transform), default(bool)));

        public static void Prefix(ExosuitTorpedoArm __instance, bool __result, TorpedoType torpedoType, Transform siloTransform)
        {
            if (torpedoType != null)
            {
                ExosuitArmAction action = ExosuitArmAction.START_USE_TOOL;
                if (siloTransform == __instance.siloSecond)
                {
                    action = ExosuitArmAction.ALT_HIT;
                }
                if (siloTransform != __instance.siloFirst && siloTransform != __instance.siloSecond)
                {
                    Log.Error("Exosuit torpedo arm siloTransform is not first or second silo " + NitroxEntity.GetId(__instance.gameObject));
                }
                NitroxServiceLocator.LocateService<ExosuitModuleEvent>().BroadcastArmAction(TechType.ExosuitTorpedoArmModule,
                    __instance,
                    action,
                    Player.main.camRoot.GetAimingTransform().forward,
                    Player.main.camRoot.GetAimingTransform().rotation
                    );
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
