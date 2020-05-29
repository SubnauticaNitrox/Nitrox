using System;
using System.Reflection;
using Harmony;
using NitroxModel.Core;
using NitroxPatcher.PatchLogic.Bases;

namespace NitroxPatcher.Patches.Dynamic
{
    public class BuilderTool_OnHoverConstructable_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(BuilderTool);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnHover", BindingFlags.NonPublic | BindingFlags.Instance,
                                                                                 null, new[] { typeof(Constructable) }, null);

        public static void Postfix(BuilderTool __instance, Constructable constructable)
        {
            NitroxServiceLocator.LocateService<Building>().BuilderTool_OnHoverConstructable_Post(__instance.gameObject, constructable);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
