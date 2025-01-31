using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class PrisonPredatorSwimToPlayer_Perform_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((PrisonPredatorSwimToPlayer t) => t.Evaluate(default(Creature), default(float)));

    internal static readonly EcoRegion.TargetFilter isTargetValidFilter = new(IsTargetValid);

    public static bool Prefix(PrisonPredatorSwimToPlayer __instance)
    {
        if (!__instance.TryGetNitroxId(out NitroxId creatureId) ||
            Resolve<SimulationOwnership>().HasAnyLockType(creatureId))
        {
            return true;
        }

        return false;
    }

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        Label jumpLabel = generator.DefineLabel();

        return new CodeMatcher(instructions).MatchStartForward([
                                                new CodeMatch((i) => i.opcode == OpCodes.Bge_Un || i.opcode == OpCodes.Bge_Un_S),
                                                new CodeMatch(OpCodes.Ldc_R4, 0f),
                                                new CodeMatch(OpCodes.Ret),
                                                new CodeMatch(OpCodes.Ldsfld, Reflect.Field(() => Player.main))
                                             ])
                                            .Set(OpCodes.Bge_Un, jumpLabel)
                                            .MatchStartForward(new CodeMatch(OpCodes.Ldsfld, Reflect.Field(() => Player.main)))
                                            .RemoveInstructions(36)
                                            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1)
                                            {
                                                labels = [jumpLabel]
                                            })
                                            .Insert(new CodeInstruction(OpCodes.Call, Reflect.Method(() => EvaluatePriority(default))))
                                            .InstructionEnumeration();
    }

    public static bool EvaluatePriority(Creature creature)
    {
        return false;
    }

    public static bool IsTargetValid(IEcoTarget ecoTarget)
    {
        GameObject target = ecoTarget.GetGameObject();
        if (!target)
        {
            return false;
        }

        if (target == Player.main.gameObject && !Player.main.CanBeAttacked())
        {
            return false;
        }

        if (target.TryGetComponent(out RemotePlayerIdentifier remotePlayerIdentifier) && !remotePlayerIdentifier.RemotePlayer.CanBeAttacked())
        {
            return false;
        }

        return true;
    }

    public static IEcoTarget GetNearestTarget(PrisonPredatorSwimToPlayer creatureAction)
    {
        IEcoTarget ecoTarget = EcoRegionManager.main.FindNearestTarget(
            RemotePlayer.PLAYER_ECO_TARGET_TYPE,
            creatureAction.transform.position,
            isTargetValidFilter,
            maxRings: 1
        );

        return ecoTarget;
    }
}
