using System.Reflection;
using Nitrox.Model.DataStructures;
using NitroxClient.GameLogic;

namespace NitroxClient.Patching.Patches.Dynamic;

/// <summary>
/// Ensures vehicle charging from modules only occurs on the simulating player.
/// </summary>
public sealed partial class Vehicle_AddEnergy_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Vehicle t) => t.AddEnergy(default));

    public static bool Prefix(Vehicle __instance)
    {
        return __instance.TryGetNitroxId(out NitroxId vehicleId) && Resolve<SimulationOwnership>().HasAnyLockType(vehicleId);
    }
}
