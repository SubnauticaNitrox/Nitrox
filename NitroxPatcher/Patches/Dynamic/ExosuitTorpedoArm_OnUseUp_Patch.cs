using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel_Subnautica.Packets;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    class ExosuitTorpedoArm_OnUseUp_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((ExosuitTorpedoArm t) => ((IExosuitArm)t).OnUseUp(out Reflect.Ref<float>.Field));

        public static void Prefix(ExosuitTorpedoArm __instance)
        {
            Resolve<ExosuitModuleEvent>().BroadcastArmAction(TechType.ExosuitTorpedoArmModule, __instance, ExosuitArmAction.END_USE_TOOL);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
