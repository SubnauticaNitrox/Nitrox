using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.Patching.Helper;

namespace NitroxClient.Patching.Patches.Dynamic;

/// <summary>
///     Broadcasts when a vehicle changes its precursorOutOfWater state
/// </summary>
internal sealed partial class PrecursorMoonPoolTrigger_Update_Patch : NitroxPatch, IDynamicPatch
{
    private static SimulationOwnership simulationOwnership;
    private static EntityMetadataManager entityMetadataManager;
    private static Entities entities;

    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((PrecursorMoonPoolTrigger t) => t.Update());

    /// <summary>
    ///     Broadcasts when a vehicle changes its precursorOutOfWater state
    /// </summary>
    public PrecursorMoonPoolTrigger_Update_Patch(SimulationOwnership so, EntityMetadataManager emm, Entities e)
    {
        simulationOwnership = so;
        entityMetadataManager = emm;
        entities = e;
    }

    /*
     * bool flag2 = y2 > num2;
     * PrecursorMoonPoolTrigger_Update_Patch.BroadcastVehicleUpdate(vehicle, flag2); <------ [INSERTED LINE]
     * vehicle.precursorOutOfWater = flag2;
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions).MatchStartForward(new CodeMatch(OpCodes.Ldloc_3), new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Stfld, Reflect.Field((Vehicle t) => t.precursorOutOfWater)))
                                            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_3)) // vehicle
                                            .InsertAndAdvance(TARGET_METHOD.Ldloc<bool>(1)) // flag2
                                            .Insert(new CodeInstruction(OpCodes.Call, Reflect.Method(() => BroadcastVehicleUpdate(default, default))))
                                            .InstructionEnumeration();
    }

    public static void BroadcastVehicleUpdate(Vehicle vehicle, bool precursorOutOfWater)
    {
        if (vehicle.precursorOutOfWater != precursorOutOfWater && vehicle.TryGetIdOrWarn(out NitroxId vehicleId) &&
            simulationOwnership.HasAnyLockType(vehicleId))
        {
            // We patch before it's actually set so we need to apply the setting manually
            vehicle.precursorOutOfWater = precursorOutOfWater;

            Optional<EntityMetadata> metadata = entityMetadataManager.Extract(vehicle);
            if (metadata.HasValue)
            {
                entities.BroadcastMetadataUpdate(vehicleId, metadata.Value);
            }
        }
    }
}
