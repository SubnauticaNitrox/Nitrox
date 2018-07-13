using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Logger;

namespace NitroxPatcher.Patches
{
    public class Vehicle_OnPilotModeEnd_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Vehicle);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnPilotModeEnd", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Prefix(Vehicle __instance)
        {
            NitroxServiceLocator.LocateService<Vehicles>().OnPilotMode(__instance, 1);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
