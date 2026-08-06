using System.Reflection;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Locked mode makes the game consider a player above water. Restore the vanilla vehicle
/// power requirement for passengers without setting currentMountedVehicle (a driver-only field).
/// </summary>
public sealed partial class Player_CanBreathe_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Player t) => t.CanBreathe());

    public static void Postfix(ref bool __result)
    {
        SeamothPassengers passengers = Resolve<SeamothPassengers>();
        if (passengers.IsPassenger && passengers.CurrentSeamoth)
        {
            __result = passengers.CurrentSeamoth.IsPowered();
        }
    }
}
