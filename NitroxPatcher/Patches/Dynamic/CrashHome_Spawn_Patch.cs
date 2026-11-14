using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class CrashHome_Spawn_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((CrashHome t) => t.Spawn());

    public static bool Prefix(CrashHome __instance)
    {
        if (__instance.TryGetNitroxId(out NitroxId crashHomeId) &&
            Resolve<SimulationOwnership>().HasAnyLockType(crashHomeId))
        {
            return true;
        }
        return false;
    }

    /*
     * this.spawnTime = -1f;
     * BroadcastFishCreated(gameObject);            [INSERTED LINE]
     * [RETURN EQUIVALENT]
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions).MatchStartForward(new CodeMatch(OpCodes.Stfld, Reflect.Field((CrashHome t) => t.spawnTime)))
                                            .Advance(1)
                                            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_0))
                                            .InsertAndAdvance(new CodeInstruction(OpCodes.Call, Reflect.Method(() => BroadcastFishCreated(default))))
                                            .RemoveInstructions(7)
                                            .InstructionEnumeration();
    }

    public static void BroadcastFishCreated(GameObject crashFishObject)
    {
        if (!crashFishObject.TryGetComponentInParent(out CrashHome crashHome, true) ||
            !crashHome.TryGetNitroxId(out NitroxId crashHomeId) || !DayNightCycle.main)
        {
            return;
        }
        NitroxId crashFishId = NitroxEntity.GenerateNewId(crashFishObject);
        LargeWorldEntity largeWorldEntity = crashFishObject.GetComponent<LargeWorldEntity>();
        UniqueIdentifier uniqueIdentifier = crashFishObject.GetComponent<UniqueIdentifier>();

        // Create the entity
        WorldEntity crashFishEntity = new(crashFishObject.transform.ToWorldDto(), (int)largeWorldEntity.cellLevel, uniqueIdentifier.classId, false, crashFishId, TechType.Crash.ToDto(), null, null, []);
        Resolve<Entities>().BroadcastEntitySpawnedByClient(crashFishEntity);

        // Forcefully unparents it from the CrashHome and puts it under its parent cell
        if (LargeWorldStreamer.main)
        {
            LargeWorldStreamer.main.cellManager.RegisterEntity(largeWorldEntity);
        }

        // Broadcast the new CrashHome's metadata ONLY after sending the entity spawn packet so that the processor always tries finding the Crash
        // after the spawning process has at least begun
        CrashHomeMetadata crashHomeMetadata = Resolve<CrashHomeMetadataExtractor>().Extract(crashHome);
        Resolve<Entities>().BroadcastMetadataUpdate(crashHomeId, crashHomeMetadata);
    }
}
