using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Vehicle_OnKill_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Vehicle);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnKill", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Prefix(Vehicle __instance)
        {
            NitroxServiceLocator.LocateService<Vehicles>().BroadcastDestroyedVehicle(__instance);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
