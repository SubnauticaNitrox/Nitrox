using System;
using System.Reflection;
using NitroxHarmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;

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

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}

