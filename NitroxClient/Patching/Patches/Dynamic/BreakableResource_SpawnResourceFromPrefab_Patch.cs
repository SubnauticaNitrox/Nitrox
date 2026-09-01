using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.Patching.PatternMatching;
using UnityEngine;
using static System.Reflection.Emit.OpCodes;

namespace NitroxClient.Patching.Patches.Dynamic;

/// <summary>
///     Synchronizes entities that can be broken and that will drop material, such as limestones...
/// </summary>
internal sealed partial class BreakableResource_SpawnResourceFromPrefab_Patch : NitroxPatch, IDynamicPatch
{
    private static Items items;
    public static readonly MethodInfo TARGET_METHOD = AccessTools.EnumeratorMoveNext(Reflect.Method(() => BreakableResource.SpawnResourceFromPrefab(default, default, default)));

    private static readonly InstructionsPattern SpawnResFromPrefPattern = new()
    {
        { Reflect.Method((Rigidbody b) => b.AddForce(default(Vector3))), "DropItemInstance" },
        Ldc_I4_0
    };

    public BreakableResource_SpawnResourceFromPrefab_Patch(Items i)
    {
        items = i ?? throw new ArgumentNullException(nameof(i));
    }

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
        items.Dropped(__instance);
    }
}
