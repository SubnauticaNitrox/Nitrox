using System.Reflection;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Ejects a local Seamoth passenger before vanilla docking reparents the first child Player
/// directly to the pilot transform, which would otherwise make the passenger a pilot.
/// </summary>
public sealed partial class Vehicle_docked_set_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Property((Vehicle t) => t.docked).SetMethod;

    public static void Prefix(Vehicle __instance, bool value)
    {
        if (value && __instance is SeaMoth)
        {
            Resolve<SeamothPassengers>().OnVehicleUnavailable(__instance, false);
        }
    }
}
