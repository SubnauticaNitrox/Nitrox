using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.Communication;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel_Subnautica.DataStructures;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxPatcher.PatternMatching;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class CyclopsDestructionEvent_SpawnLootAsync_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = AccessTools.EnumeratorMoveNext(Reflect.Method((CyclopsDestructionEvent t) => t.SpawnLootAsync()));

    public static readonly InstructionsPattern PATTERN = new(expectedMatches: 2)
    {
        { Reflect.Method(() => UnityEngine.Object.Instantiate(default(GameObject), default(Vector3), default(Quaternion))), "SpawnObject" }
    };

    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
    {
        return instructions.InsertAfterMarker(PATTERN, "SpawnObject", [
            new(OpCodes.Dup),
            new(OpCodes.Ldloc_1),
            new(OpCodes.Call, ((Action<GameObject, CyclopsDestructionEvent>)Callback).Method)
        ]);
    }

    public static void Callback(GameObject gameObject, CyclopsDestructionEvent __instance)
    {
        NitroxId lootId = NitroxEntity.GenerateNewId(gameObject);

        LargeWorldEntity largeWorldEntity = gameObject.GetComponent<LargeWorldEntity>();
        PrefabIdentifier prefabIdentifier = gameObject.GetComponent<PrefabIdentifier>();
        Pickupable pickupable = gameObject.GetComponent<Pickupable>();

        WorldEntity lootEntity = new(gameObject.transform.ToWorldDto(), (int)largeWorldEntity.cellLevel, prefabIdentifier.classId, false, lootId, pickupable.GetTechType().ToDto(), null, null, []);
        Resolve<Entities>().BroadcastEntitySpawnedByClient(lootEntity);
    }
}
