using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    class VehicleDockingBay_OnUndockingComplete_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((VehicleDockingBay t) => t.OnUndockingComplete(default(Player)));

        public static void Prefix(VehicleDockingBay __instance, Player player)
        {
            Vehicle vehicle = __instance.GetDockedVehicle();
            Resolve<Vehicles>().BroadcastVehicleUndocking(__instance, vehicle, false);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
