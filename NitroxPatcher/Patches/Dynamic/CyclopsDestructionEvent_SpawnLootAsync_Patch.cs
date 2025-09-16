using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using NitroxPatcher.PatternMatching;
using UnityEngine;
using static System.Reflection.Emit.OpCodes;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class CyclopsDestructionEvent_SpawnLootAsync_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = AccessTools.EnumeratorMoveNext(Reflect.Method((CyclopsDestructionEvent t) => t.SpawnLootAsync()));

    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
               .MatchStartForward(new CodeMatch(Switch))
               .InsertAndAdvance(new CodeInstruction(Call, Reflect.Method(() => TrampolineCallback(default))))
               .InstructionEnumeration()
               // Matches twice, once for scrap metal and once for computer chips
               .RewriteOnPattern([
                   Reflect.Method(() => UnityEngine.Object.Instantiate(default(GameObject), default(Vector3), default(Quaternion))),
                   [
                       Dup,
                       Ldloc_1,
                       ((Action<GameObject, CyclopsDestructionEvent>)SpawnObjectCallback).Method
                   ]
               ], 2);
    }

    public static void SpawnObjectCallback(GameObject gameObject, CyclopsDestructionEvent __instance)
    {
        NitroxId lootId = NitroxEntity.GenerateNewId(gameObject);

        LargeWorldEntity largeWorldEntity = gameObject.GetComponent<LargeWorldEntity>();
        PrefabIdentifier prefabIdentifier = gameObject.GetComponent<PrefabIdentifier>();
        Pickupable pickupable = gameObject.GetComponent<Pickupable>();

        WorldEntity lootEntity = new(gameObject.transform.ToWorldDto(), (int)largeWorldEntity.cellLevel, prefabIdentifier.classId, false, lootId, pickupable.GetTechType().ToDto(), null, null, []);
        Resolve<Entities>().BroadcastEntitySpawnedByClient(lootEntity);
    }

    public static int TrampolineCallback(int originalIndex)
    {
        // Immediately return from iterator block if called from within CyclopsMetadataProcessor
        return PacketSuppressor<EntitySpawnedByClient>.IsSuppressed ? int.MaxValue : originalIndex;
    }
}
