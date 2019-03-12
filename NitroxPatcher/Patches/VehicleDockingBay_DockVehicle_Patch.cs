using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Logger;

namespace NitroxPatcher.Patches
{
    public class VehicleDockingBay_DockVehicle_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(VehicleDockingBay);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("DockVehicle");

        public static bool Prefix(VehicleDockingBay __instance, Vehicle vehicle)
        {
            NitroxServiceLocator.LocateService<Vehicles>().BroadcastVehicleDocking(__instance, vehicle);
            return true;
        }

        public static void Postfix(VehicleDockingBay __instance, Vehicle vehicle)
        {
            NitroxServiceLocator.LocateService<Vehicles>().BroadcastVehicleDocking(__instance, vehicle);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}

