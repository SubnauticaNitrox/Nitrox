using System;
using System.Reflection;
using Harmony;
using NitroxModel.Core;
using NitroxPatcher.PatchLogic.Bases;

namespace NitroxPatcher.Patches.Dynamic
{
    public class BuilderTool_OnHoverBaseDeconstructable_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(BuilderTool);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnHover", BindingFlags.NonPublic | BindingFlags.Instance,
                                                                                 null, new[] { typeof(BaseDeconstructable) }, null);

        public static void Postfix(BuilderTool __instance, BaseDeconstructable deconstructable)
        {
            NitroxServiceLocator.LocateService<Building>().BuilderTool_OnHoverDeconstructable_Post(__instance.gameObject, deconstructable);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }

}
