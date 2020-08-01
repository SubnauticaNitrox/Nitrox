using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Exosuit_ArmSpawned_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Exosuit);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("UpdateColliders", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(Exosuit __instance)
        {
            NitroxServiceLocator.LocateService<ExosuitModuleEvent>().SpawnedArm(__instance);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}

