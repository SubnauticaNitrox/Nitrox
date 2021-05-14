using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    class VehicleDockingBay_OnUndockingComplete_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(VehicleDockingBay);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnUndockingComplete", BindingFlags.Public | BindingFlags.Instance);

        public static void Prefix(VehicleDockingBay __instance, Player player)
        {
#if SUBNAUTICA
            Vehicle vehicle = __instance.GetDockedVehicle();
#elif BELOWZERO
            Vehicle vehicle = __instance.GetDockedObject().vehicle;
#endif

            NitroxServiceLocator.LocateService<Vehicles>().BroadcastVehicleUndocking(__instance, vehicle, false);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
