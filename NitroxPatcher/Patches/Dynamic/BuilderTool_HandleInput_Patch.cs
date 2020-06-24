using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    public class BuilderTool_HandleInput_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(BuilderTool);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("HandleInput", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool Prefix(BuilderTool __instance)
        {
            NitroxServiceLocator.LocateService<Building>().currentlyHandlingBuilderTool = true;
            return true;
        }

        public static void Postfix(BuilderTool __instance)
        {
            NitroxServiceLocator.LocateService<Building>().currentlyHandlingBuilderTool = false;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, true, true, false);
        }
    }
}
