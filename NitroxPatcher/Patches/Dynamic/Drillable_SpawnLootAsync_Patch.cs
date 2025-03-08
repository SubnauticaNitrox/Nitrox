using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using NitroxPatcher.PatternMatching;
using UnityEngine;
using static System.Reflection.Emit.OpCodes;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Sync entities spawned from drilling large resource deposits.
/// </summary>
public sealed partial class Drillable_SpawnLootAsync_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = AccessTools.EnumeratorMoveNext(Reflect.Method((Drillable t) => t.SpawnLootAsync(default)));

    private static readonly InstructionsPattern Pattern = new()
    {
        { Reflect.Method((Rigidbody b) => b.AddTorque(default)), "AddTorque" }
    };

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return instructions.InsertAfterMarker(Pattern, "AddTorque", [
            new(Ldloc_3),
            new(Call, ((Action<GameObject>)Callback).Method)
        ]);
    }

    private static void Callback(GameObject instance)
    {
        NitroxEntity.SetNewId(instance, new());
        Resolve<Items>().Dropped(instance);
    }
}
