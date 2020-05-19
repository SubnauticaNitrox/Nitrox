using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Constructable_NotifyConstructedChanged_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Constructable);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("NotifyConstructedChanged", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(Constructable __instance)
        {
            NitroxServiceLocator.LocateService<Building>().Constructable_NotifyConstructedChanged_Post(__instance);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
