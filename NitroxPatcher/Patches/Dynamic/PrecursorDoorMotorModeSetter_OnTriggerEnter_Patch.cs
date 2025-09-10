using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Util;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Broadcasts when an exosuit changes its precursorOutOfWater state
/// </summary>
public sealed partial class PrecursorDoorMotorModeSetter_OnTriggerEnter_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((PrecursorDoorMotorModeSetter t) => t.OnTriggerEnter(default));

    /*
     * Exosuit componentInHierarchy2 = global::UWE.Utils.GetComponentInHierarchy<Exosuit>(gameObject);
     * PrecursorDoorMotorModeSetter_OnTriggerEnter_Patch.BroadcastExosuitUpdate(componentInHierarchy2, this); <------ [INSERTED LINE]
     * if (componentInHierarchy2)
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions).MatchEndForward([
                                                new CodeMatch(OpCodes.Ldloc_0),
                                                new CodeMatch(OpCodes.Call),
                                                new CodeMatch(OpCodes.Stloc_2),
                                            ])
                                            .Advance(1)
                                            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_2)) // componentInHierarchy2 (Exosuit)
                                            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0)) // this (PrecursorDoorMotorModeSetter)
                                            .Insert(new CodeInstruction(OpCodes.Call, Reflect.Method(() => BroadcastExosuitUpdate(default, default))))
                                            .InstructionEnumeration();
    }

    public static void BroadcastExosuitUpdate(Exosuit exosuit, PrecursorDoorMotorModeSetter precursorDoorMotorModeSetter)
    {
        bool precursorOutOfWater = precursorDoorMotorModeSetter.setToMotorModeOnEnter == PrecursorDoorMotorMode.ForceWalk;
        if (exosuit && exosuit.precursorOutOfWater != precursorOutOfWater && exosuit.TryGetIdOrWarn(out NitroxId exosuitId) &&
            Resolve<SimulationOwnership>().HasAnyLockType(exosuitId))
        {
            // We patch before it's actually set so we need to apply the setting manually
            exosuit.precursorOutOfWater = precursorOutOfWater;
            Optional<ExosuitMetadata> metadata = Resolve<ExosuitMetadataExtractor>().Extract(exosuit);
            if (metadata.HasValue)
            {
                Resolve<Entities>().BroadcastMetadataUpdate(exosuitId, metadata.Value);
            }
        }
    }
}
