using HarmonyLib;
using Mono.Cecil.Cil;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxPatcher.PatternMatching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static System.Reflection.Emit.OpCodes;

namespace NitroxPatcher.Patches.Dynamic;

internal class BreakableResource_SpawnResourceFromPrefab_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD_ORIGINAL = Reflect.Method((BreakableResource t) => t.SpawnResourceFromPrefab(default));
    public static readonly MethodInfo TARGET_METHOD = AccessTools.EnumeratorMoveNext(TARGET_METHOD_ORIGINAL);

    private static readonly InstructionsPattern injectionInstruction = new()
    {
        Reflect.Method((Rigidbody r) => r.AddForce(default(Vector3))),
        { Ldloc_1, "Callback" },
        Ldc_I4_0
    };

    public static readonly object INJECTION_OPERAND = Reflect.Method(() => BreakableResource.SpawnResourceFromPrefab(default, default, default));

    

    public IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
    {
        static IEnumerable<CodeInstruction> InsertCallback(string label, CodeInstruction _)
        {
            switch(label)
            {
                case "Callback":
                    yield return new(Ldloc_1);
                    yield return new(Call, Reflect.Method(() => ));
                    break;
            }
        }
    }

    private static void Callback(GameObject __instance)
    {
        NitroxId newId = new();
        NitroxEntity.SetNewId(__instance, newId);
        Resolve<Items>().Dropped(__instance, CraftData.GetTechType(__instance));
    }

    public override void Patch(Harmony harmony)
    {
        PatchTranspiler(harmony, TARGET_METHOD);
    }
}
