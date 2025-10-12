using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Ensures vehicle charging from moonpool only occurs on the simulating player.
/// </summary>
public sealed partial class Vehicle_UpdateEnergyRecharge_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Vehicle t) => t.UpdateEnergyRecharge());

    public static bool Prefix(Vehicle __instance)
    {
        return __instance.TryGetNitroxId(out NitroxId vehicleId) && Resolve<SimulationOwnership>().HasAnyLockType(vehicleId);
    }
}
