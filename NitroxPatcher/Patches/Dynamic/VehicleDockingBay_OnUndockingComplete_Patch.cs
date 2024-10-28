using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class VehicleDockingBay_OnUndockingComplete_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo targetMethod = Reflect.Method((VehicleDockingBay t) => t.OnUndockingComplete(default(Player)));

    public static void Prefix(VehicleDockingBay __instance)
    {
        Resolve<Vehicles>().BroadcastVehicleUndocking(__instance, __instance.GetDockedVehicle(), false);
    }
}
