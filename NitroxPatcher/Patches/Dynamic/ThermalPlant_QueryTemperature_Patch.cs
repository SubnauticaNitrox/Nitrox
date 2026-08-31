using System;
using System.Reflection;
using Nitrox.Model.DataStructures;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Broadcasts and persists <see cref="ThermalPlant.temperature"/> whenever it rises, so thermal plants keep generating
/// power (and the temperature reached is remembered) across respawns and server restarts instead of resetting to 0.
/// </summary>
public sealed partial class ThermalPlant_QueryTemperature_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((ThermalPlant t) => t.QueryTemperature());

    public static void Prefix(ThermalPlant __instance, out float __state)
    {
        __state = __instance.temperature;
    }

    public static void Postfix(ThermalPlant __instance, float __state)
    {
        // temperature only ever increases (see ThermalPlant.QueryTemperature's Mathf.Max). Broadcast only once per whole
        // degree gained: the water temperature creeps toward its daily peak in ever smaller steps, so without this every
        // nearby client would send a near-duplicate update every QueryTemperature tick. Power output only depends on
        // Mathf.InverseLerp(25, 100, temperature), so sub-degree changes don't matter.
        if (Math.Floor(__instance.temperature) > Math.Floor(__state) && __instance.TryGetNitroxId(out NitroxId entityId))
        {
            Resolve<Entities>().EntityMetadataChanged(__instance, entityId);
        }
    }
}
