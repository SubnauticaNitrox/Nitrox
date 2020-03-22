using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel_Subnautica.Packets;

namespace NitroxPatcher.Patches.Dynamic
{
    class ExosuitGrapplingArm_OnUseUp_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(ExosuitGrapplingArm);
        public static readonly Type TARGET_INTERFACE = typeof(IExosuitArm);
        public static readonly MethodInfo TARGET_METHOD_INTERFACE = typeof(IExosuitArm).GetMethod("OnUseUp");

        public static void Prefix(ExosuitGrapplingArm __instance)
        {
            NitroxServiceLocator.LocateService<ExosuitModuleEvent>().BroadcastArmAction(TechType.ExosuitGrapplingArmModule, __instance, ExosuitArmAction.END_USE_TOOL);
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
