using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    public class BaseRoot_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(BaseRoot);
        public static readonly ConstructorInfo TARGET_METHOD = TARGET_CLASS.GetConstructor(new Type[] { });

        public static void Postfix(BaseRoot __instance)
        {
            NitroxServiceLocator.LocateService<Building>().BaseRoot_Constructor_Post(__instance);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
