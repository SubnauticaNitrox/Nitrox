using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class PrisonPredatorSwimToPlayer_Perform_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((PrisonPredatorSwimToPlayer t) => t.Perform(default, default, default));

    /// <summary>
    /// Replace all the method with our custom <see cref="Perform(PrisonPredatorSwimToPlayer, Creature, float)"/>
    /// 
    /// Original method does hardcode Player.main usage and doesn't use any "GameObject" or Target abstraction as in other <see cref="CreatureAction"/>
    /// So we need to rewrite <see cref="PrisonPredatorSwimToPlayer.Evaluate(Creature, float)"/> to use the same logic as <see cref="PrisonPredatorSwimToPlayer.Perform(Creature, float)"/>"/>
    /// </summary>
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        yield return new CodeInstruction(OpCodes.Ldarg_0);
        yield return new CodeInstruction(OpCodes.Ldarg_1);
        yield return new CodeInstruction(OpCodes.Ldarg_2);
        yield return new CodeInstruction(OpCodes.Call, Reflect.Method(() => Perform(default, default, default)));
        yield return new CodeInstruction(OpCodes.Ret);
    }

    public static void Perform(PrisonPredatorSwimToPlayer instance, Creature creature, float time)
    {
        LastTarget lastTarget = creature.GetComponent<LastTarget>();
        if (!lastTarget)
        {
            Log.Error($"Creature {creature} does not have a LastTarget component.");
            return;
        }

        Transform targetTransform = lastTarget.target ? lastTarget.target.transform : null;
        if (!targetTransform)
        {
            return;
        }

        if (time > instance.timeNextSwim)
        {
            instance.swimBehaviour.SwimTo(targetTransform.position, instance.swimVelocity);
            creature.TryGetNitroxId(out NitroxId nitroxId);

            Log.InGame($"[PrisonPredatorSwimTo Swim] {creature.name} {nitroxId?.ToString()} {lastTarget.target.name}");
        }
    }
}
