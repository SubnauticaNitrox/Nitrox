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
public class BreakableResource_SpawnResourceFromPrefab_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD_ORIGINAL = Reflect.Method(() => BreakableResource.SpawnResourceFromPrefab(default, default, default));
    public static readonly MethodInfo TARGET_METHOD = AccessTools.EnumeratorMoveNext(TARGET_METHOD_ORIGINAL);

    private static readonly InstructionsPattern SpawnResFromPrefPattern = new()
    {
        { Reflect.Method((Rigidbody b) => b.AddForce(default(Vector3))), "DropItemInstance" },
        Ldc_I4_0
    };

    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
    {
        static IEnumerable<CodeInstruction> InsertCallback(string label, CodeInstruction _)
        {
            switch(label)
            {
                case "DropItemInstance":
                    yield return new(Ldloc_1);
                    yield return new(Call, Reflect.Method(() => Callback(default)));
                    break;
            }
        }
        return instructions.Transform(SpawnResFromPrefPattern, InsertCallback);
    }

    private static void Callback(GameObject __instance)
    {
        NitroxEntity.SetNewId(__instance, new());
        Resolve<Items>().Dropped(__instance);
    }

    public override void Patch(Harmony harmony)
    {
        PatchTranspiler(harmony, TARGET_METHOD);
    }
}
