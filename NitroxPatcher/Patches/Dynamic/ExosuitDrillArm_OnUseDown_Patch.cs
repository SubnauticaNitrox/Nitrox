using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel_Subnautica.Packets;

namespace NitroxPatcher.Patches.Dynamic
{
    class ExosuitDrillArm_OnUseDown_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(ExosuitDrillArm);
        public static readonly Type TARGET_INTERFACE = typeof(IExosuitArm);
        public static readonly MethodInfo TARGET_METHOD_INTERFACE = TARGET_INTERFACE.GetMethod(nameof(IExosuitArm.OnUseDown));

        public static void Prefix(ExosuitDrillArm __instance)
        {
            Resolve<ExosuitModuleEvent>().BroadcastArmAction(TechType.ExosuitDrillArmModule, __instance, ExosuitArmAction.START_USE_TOOL);
        }

        public override void Patch(Harmony harmony)
        {
            InterfaceMapping interfaceMap = TARGET_CLASS.GetInterfaceMap(TARGET_INTERFACE);
            int i = Array.IndexOf(interfaceMap.InterfaceMethods, TARGET_METHOD_INTERFACE);
            MethodInfo targetMethod = interfaceMap.TargetMethods[i];

            PatchPrefix(harmony, targetMethod);
        }
    }
}
