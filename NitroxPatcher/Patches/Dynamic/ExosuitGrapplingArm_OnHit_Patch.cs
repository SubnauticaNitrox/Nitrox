﻿using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.Packets;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    class ExosuitGrapplingArm_OnHit_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(ExosuitGrapplingArm);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnHit");

        public static bool Prefix(ExosuitGrapplingArm __instance, GrapplingHook ___hook)
        {
            Exosuit componentInParent = __instance.GetComponentInParent<Exosuit>();
            
            if(componentInParent != null )
            {
                if(!componentInParent.GetPilotingMode())
                {
                    // We suppress this method if it is called from another player pilot, so we can use our own implementation
                    // See: ExosuitModuleEvents.UseGrapplingarm -> onHit Section
                    return false;
                }
            }
            
            return true;
        }

        public static void Postfix(ExosuitGrapplingArm __instance, GrapplingHook ___hook)
        {
            // We send the hook direction to the other player so he sees where the other player exosuit is heading
            NitroxServiceLocator.LocateService<ExosuitModuleEvent>().BroadcastArmAction(TechType.ExosuitGrapplingArmModule, __instance, ExosuitArmAction.START_USE_TOOL, ___hook.rb.velocity, null);
        }

        public override void Patch(Harmony harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, true, true, false, false);            
        }
    }
}
