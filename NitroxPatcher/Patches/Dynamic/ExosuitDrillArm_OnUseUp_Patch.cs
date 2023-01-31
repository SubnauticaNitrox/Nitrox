using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel_Subnautica.Packets;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    class ExosuitDrillArm_OnUseUp_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((ExosuitDrillArm t) => ((IExosuitArm)t).OnUseUp(out Reflect.Ref<float>.Field));

        public static void Prefix(ExosuitDrillArm __instance)
        {
            NitroxServiceLocator.LocateService<ExosuitModuleEvent>().BroadcastArmAction(TechType.ExosuitDrillArmModule, __instance, ExosuitArmAction.END_USE_TOOL);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
