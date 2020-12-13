using System;
using System.Reflection;
using Harmony;
using Nitrox.Client.GameLogic;
using Nitrox.Model.Core;

namespace Nitrox.Patcher.Patches.Dynamic
{
    class VehicleDockingBay_OnUndockingComplete_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(VehicleDockingBay);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnUndockingComplete", BindingFlags.Public | BindingFlags.Instance);

        public static void Prefix(VehicleDockingBay __instance, Player player)
        {
            Vehicle vehicle = __instance.GetDockedVehicle();

            NitroxServiceLocator.LocateService<Vehicles>().BroadcastVehicleUndocking(__instance, vehicle, false);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
