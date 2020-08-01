using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Vehicle_OnPilotModeBegin_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Vehicle);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnPilotModeBegin", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Prefix(Vehicle __instance)
        {
            NitroxServiceLocator.LocateService<Vehicles>().BroadcastOnPilotModeChanged(__instance, true);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
