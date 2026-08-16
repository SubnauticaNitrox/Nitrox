using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Spawning.Metadata;

namespace NitroxClient.Patching.Patches.Dynamic;

internal sealed partial class CrashHome_Update_Patch : NitroxPatch, IDynamicPatch
{
    private static SimulationOwnership simulationOwnership;
    private static EntityMetadataManager entityMetadataManager;
    private static Entities entities;
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((CrashHome t) => t.Update());

    public CrashHome_Update_Patch(SimulationOwnership so, EntityMetadataManager emm, Entities e)
    {
        simulationOwnership = so ?? throw new ArgumentNullException(nameof(so));
        entityMetadataManager = emm ?? throw new ArgumentNullException(nameof(emm));
        entities = e ?? throw new ArgumentNullException(nameof(e));
    }

    /*
     * if (!this.crash && this.spawnTime < 0f)
     * {
     *     this.spawnTime = (float)(main.timePassed + 1200.0);  [REMOVED LINE]
     *     UpdateSpawnTimeAndBroadcast(this);                   [INSERTED LINE]
     * }
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions).MatchEndForward(new CodeMatch(OpCodes.Ldfld), new CodeMatch(OpCodes.Ldc_R4), new CodeMatch(OpCodes.Bge_Un))
                                            .Advance(1)
                                            .RemoveInstructions(7)
                                            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                                            .InsertAndAdvance(new CodeInstruction(OpCodes.Call, Reflect.Method(() => UpdateSpawnTimeAndBroadcast(default))))
                                            .InstructionEnumeration();
    }

    public static void UpdateSpawnTimeAndBroadcast(CrashHome crashHome)
    {
        // We update and broadcast the spawn time only if we're simulating the home
        if (!crashHome.TryGetNitroxId(out NitroxId crashHomeId) ||
            !simulationOwnership.HasAnyLockType(crashHomeId))
        {
            return;
        }

        crashHome.spawnTime = DayNightCycle.main.timePassedAsFloat + (float)CrashHome.respawnDelay;

        // Set spawn time before broadcast the new CrashHome's metadata
        Optional<EntityMetadata> metadata = entityMetadataManager.Extract(crashHome);
        if (metadata.HasValue)
        {
            entities.BroadcastMetadataUpdate(crashHomeId, metadata.Value);
        }
    }
}
