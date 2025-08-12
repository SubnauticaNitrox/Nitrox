using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class CrashHome_Update_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((CrashHome t) => t.Update());

    /*
     * if (!this.crash && this.spawnTime < 0f)
     * {
     *     this.spawnTime = (float)(main.timePassed + 1200.0);  [REMOVED LINE]
     *     UpdateSpawnTimeAndBroadcast(this);                   [INSERTED LINE]
     * }
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions).MatchEndForward([
                                                new CodeMatch(OpCodes.Ldfld),
                                                new CodeMatch(OpCodes.Ldc_R4),
                                                new CodeMatch(OpCodes.Bge_Un)
                                            ])
                                            .Advance(1)
                                            .RemoveInstructions(7)
                                            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                                            .InsertAndAdvance(new CodeInstruction(OpCodes.Call, Reflect.Method(() => UpdateSpawnTimeAndBroadcast(default))))
                                            .InstructionEnumeration();
    }

    public static void UpdateSpawnTimeAndBroadcast(CrashHome crashHome)
    {
        // We udpate and broadcast the spawn time only if we're simulating the home
        if (!crashHome.TryGetNitroxId(out NitroxId crashHomeId) ||
            !Resolve<SimulationOwnership>().HasAnyLockType(crashHomeId))
        {
            return;
        }

        crashHome.spawnTime = DayNightCycle.main.timePassedAsFloat + (float)CrashHome.respawnDelay;

        // Set spawn time before broadcast the new CrashHome's metadata
        Optional<EntityMetadata> metadata = Resolve<EntityMetadataManager>().Extract(crashHome);
        if (metadata.HasValue)
        {
            Resolve<Entities>().BroadcastMetadataUpdate(crashHomeId, metadata.Value);
        }
    }
}
