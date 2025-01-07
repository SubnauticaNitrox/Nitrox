using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.MonoBehaviours.Cyclops;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// For some reason, when a creature attacks a Cyclops, its main rigidbody is sometimes not properly detected by the raycasts.
/// Thus we change the target check (when it's a Cyclops) to also check if the collider is part of the target cyclops.
/// </summary>
public sealed partial class MeleeAttack_CanDealDamageTo_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((MeleeAttack t) => t.CanDealDamageTo(default));

    /*
     * REPLACE:
     * if (!(gameObject == target) && !(gameObject == base.gameObject) && !(gameObject.GetComponent<Creature>() != null))
     * 
     * BY:
     * if (!(gameObject == target) && !(gameObject == base.gameObject) && !(gameObject.GetComponent<Creature>() != null) && !MeleeAttack_CanDealDamageTo_Patch.IsValidTarget(target, gameObject))
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // Match the latest part of the if
        CodeMatcher matcher = new CodeMatcher(instructions).MatchEndForward([
            new CodeMatch(OpCodes.Ldloc_S),
            new CodeMatch(OpCodes.Callvirt, Reflect.Method((GameObject t) => t.GetComponent<Creature>())),
            new CodeMatch(OpCodes.Ldnull),
            new CodeMatch(OpCodes.Call),
            new CodeMatch(OpCodes.Brtrue),
        ]);

        // We copy the "brtrue" so we can easily insert another clause in the "if"
        CodeInstruction brTrueInstruction = matcher.Instruction;

        // Advance 1 to insert after the matched pattern
        matcher.Advance(1)
               .Insert([
                   new CodeInstruction(OpCodes.Ldarg_1), // target
                   new CodeInstruction(OpCodes.Ldloc_S, (byte)7), // gameObject
                   new CodeInstruction(OpCodes.Call, Reflect.Method(() => IsValidTarget(default, default))),
                   brTrueInstruction
                ]);
        
        return matcher.InstructionEnumeration();
    }

    public static bool IsValidTarget(GameObject target, GameObject gameObject)
    {
        return target.GetComponent<NitroxCyclops>() && gameObject.GetComponentInParent<NitroxCyclops>(true);
    }
}
