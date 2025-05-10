using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
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

    private static readonly InstructionsPattern pattern = new()
    {
        { Reflect.Method((Rigidbody b) => b.AddTorque(default)), "AddTorque" }
    };

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return instructions.InsertAfterMarker(pattern, "AddTorque", [
            new CodeInstruction(Ldloc_3),
            new CodeInstruction(Call, ((Action<GameObject>)Callback).Method)
        ]);
    }

    private static void Callback(GameObject instance)
    {
        Resolve<Items>().Dropped(instance);
    }
}
