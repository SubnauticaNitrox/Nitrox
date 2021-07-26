using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel_Subnautica.Packets;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    class ExosuitTorpedoArm_Shoot_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(ExosuitTorpedoArm);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Shoot", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Prefix(ExosuitTorpedoArm __instance, bool __result, TorpedoType torpedoType, Transform siloTransform)
        {
            if(torpedoType != null)
            {
                ExosuitArmAction action = ExosuitArmAction.START_USE_TOOL;
                if(siloTransform == __instance.siloSecond)
                {
                    action = ExosuitArmAction.ALT_HIT;
                }
                if(siloTransform != __instance.siloFirst && siloTransform != __instance.siloSecond)
                {
                    Log.Error("Exosuit torpedo arm siloTransform is not first or second silo " + NitroxEntity.GetId(__instance.gameObject));
                }
                NitroxServiceLocator.LocateService<ExosuitModuleEvent>().BroadcastArmAction(TechType.ExosuitTorpedoArmModule,
                    __instance, 
                    action, 
                    Optional.Of(Player.main.camRoot.GetAimingTransform().forward), 
                    Optional.Of(Player.main.camRoot.GetAimingTransform().rotation)
                    );
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
