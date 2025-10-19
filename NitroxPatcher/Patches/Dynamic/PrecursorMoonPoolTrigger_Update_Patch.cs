using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Spawning.Metadata;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic.Entities.Metadata;
using Nitrox.Model.DataStructures.Util;
using NitroxPatcher.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Broadcasts when a vehicle changes its precursorOutOfWater state
/// </summary>
public sealed partial class PrecursorMoonPoolTrigger_Update_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((PrecursorMoonPoolTrigger t) => t.Update());

    /*
     * bool flag2 = y2 > num2;
     * PrecursorMoonPoolTrigger_Update_Patch.BroadcastVehicleUpdate(vehicle, flag2); <------ [INSERTED LINE]
     * vehicle.precursorOutOfWater = flag2;
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions).MatchStartForward([
                                                new CodeMatch(OpCodes.Ldloc_3),
                                                new CodeMatch(OpCodes.Ldloc_S),
                                                new CodeMatch(OpCodes.Stfld, Reflect.Field((Vehicle t) => t.precursorOutOfWater)),
                                            ])
                                            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_3)) // vehicle
                                            .InsertAndAdvance(TARGET_METHOD.Ldloc<bool>(1)) // flag2
                                            .Insert(new CodeInstruction(OpCodes.Call, Reflect.Method(() => BroadcastVehicleUpdate(default, default))))
                                            .InstructionEnumeration();
    }

    public static void BroadcastVehicleUpdate(Vehicle vehicle, bool precursorOutOfWater)
    {
        if (vehicle.precursorOutOfWater != precursorOutOfWater && vehicle.TryGetIdOrWarn(out NitroxId vehicleId) &&
            Resolve<SimulationOwnership>().HasAnyLockType(vehicleId))
        {
            // We patch before it's actually set so we need to apply the setting manually
            vehicle.precursorOutOfWater = precursorOutOfWater;

            Optional<EntityMetadata> metadata = Resolve<EntityMetadataManager>().Extract(vehicle);
            if (metadata.HasValue)
            {
                Resolve<Entities>().BroadcastMetadataUpdate(vehicleId, metadata.Value);
            }
        }
    }
}
