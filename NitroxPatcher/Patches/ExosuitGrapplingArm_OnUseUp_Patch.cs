using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel_Subnautica.Packets;

namespace NitroxPatcher.Patches
{
    class ExosuitGrapplingArm_OnUseUp_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(ExosuitGrapplingArm);
        public static readonly Type TARGET_INTERFACE = typeof(IExosuitArm);
        public static readonly MethodInfo TARGET_METHOD_INTERFACE = typeof(IExosuitArm).GetMethod("OnUseUp");

        public static void Prefix(ExosuitGrapplingArm __instance)
        {
            NitroxServiceLocator.LocateService<ExosuitModuleEvent>().BroadcastArmAction(TechType.ExosuitGrapplingArmModule, __instance, ExosuitArmAction.endUseTool);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            InterfaceMapping interfaceMap = TARGET_CLASS.GetInterfaceMap(TARGET_INTERFACE);
            int i = Array.IndexOf(interfaceMap.InterfaceMethods, TARGET_METHOD_INTERFACE);
            MethodInfo targetMethod = interfaceMap.TargetMethods[i];

            PatchPrefix(harmony, targetMethod);
        }
    }
}
