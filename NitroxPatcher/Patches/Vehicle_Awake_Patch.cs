using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Core;

namespace NitroxPatcher.Patches
{
    public class Vehicle_Awake_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Vehicle);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Awake", BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(Vehicle __instance)
        {
            NitroxServiceLocator.LocateService<Vehicles>().CreateNewVehicle(__instance);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
