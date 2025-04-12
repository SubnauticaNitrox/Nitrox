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

public sealed partial class PrisonPredatorSwimToPlayer_Evaluate_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((PrisonPredatorSwimToPlayer t) => t.Evaluate(default(Creature), default(float)));

    internal static readonly EcoRegion.TargetFilter isTargetValidFilter = new(IsTargetValid);

    /// <summary>
    /// Replace all the method with our custom <see cref="EvaluatePriority(PrisonPredatorSwimToPlayer, Creature, float)" />
    /// 
    /// Original method does hardcode Player.main usage and doesn't use any "GameObject" or Target abstraction as in other <see cref="CreatureAction"/>
    /// So we need to rewrite <see cref="PrisonPredatorSwimToPlayer.Evaluate(Creature, float)"/> to use the same logic as <see cref="PrisonPredatorSwimToPlayer.Perform(Creature, float)"/>"/>
    /// </summary>
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        yield return new CodeInstruction(OpCodes.Ldarg_0);
        yield return new CodeInstruction(OpCodes.Ldarg_1);
        yield return new CodeInstruction(OpCodes.Ldarg_2);
        yield return new CodeInstruction(OpCodes.Call, Reflect.Method(() => EvaluatePriority(default, default, default)));
        yield return new CodeInstruction(OpCodes.Ret);
    }

    public static float EvaluatePriority(PrisonPredatorSwimToPlayer instance, Creature creature, float time)
    {
        if (time < instance.timeStopSwim + instance.pauseInterval)
        {
            return 0f;
        }

        if (time < instance.timeStartSwim + instance.maxDuration)
        {
            return 0f;
        }

        IEcoTarget ecoTarget = GetNearestTarget(instance);
        if (ecoTarget is null)
        {
            return 0f;
        }

        GameObject gameobject = ecoTarget.GetGameObject();
        if (!gameobject)
        {
            return 0f;
        }

        float num = Vector3.Distance(gameobject.transform.position, instance.transform.position);
        if (num > instance.maxDistance || num < instance.minDistance)
        {
            return 0f;
        }

        if (!creature.GetCanSeeObject(gameobject))
        {
            return 0f;
        }

        LastTarget lastTarget = creature.GetComponent<LastTarget>();
        if (!lastTarget)
        {
            Log.Error($"Creature {creature} does not have a LastTarget component.");
            return 0f;
        }

        creature.TryGetNitroxId(out NitroxId nitroxId);

        Log.InGame($"[PrisonPredatorSwimTo Eval] {creature.name} {nitroxId?.ToString()} {gameobject.name}");
        lastTarget.SetTarget(gameobject);

        return instance.GetEvaluatePriority();
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
            maxRings: 2
        );

        return ecoTarget;
    }
}
