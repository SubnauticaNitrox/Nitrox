using System;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using NitroxPatcher.PatternMatching;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static System.Reflection.Emit.OpCodes;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Synchronizes entities that can be broken and that will drop material, such as limestones...
/// </summary>
public sealed partial class BreakableResource_SpawnResourceFromPrefab_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = AccessTools.EnumeratorMoveNext(Reflect.Method(() => BreakableResource.SpawnResourceFromPrefab(default, default, default)));

    private static readonly InstructionsPattern SpawnResFromPrefPattern = new()
    {
        { Reflect.Method((Rigidbody b) => b.AddForce(default(Vector3))), "DropItemInstance" },
        Ldc_I4_0
    };

    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
    {
        return instructions.InsertAfterMarker(SpawnResFromPrefPattern, "DropItemInstance", new CodeInstruction[]
        {
            new(Ldloc_1),
            new(Call, ((Action<GameObject>)Callback).Method)
        });
    }

    private static void Callback(GameObject __instance)
    {
        NitroxEntity.SetNewId(__instance, new());
        Resolve<Items>().Dropped(__instance);
    }
}
