using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Spawning.Bases;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.Cyclops;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxPatcher.PatternMatching;
using UnityEngine;
using static System.Reflection.Emit.OpCodes;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Builder_TryPlace_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => Builder.TryPlace());

#if SUBNAUTICA
    public static readonly InstructionsPattern AddInstructionPattern1 = new()
    {
        Ldloc_0,
        Ldc_I4_0,
        Ldc_I4_1,
        new() { OpCode = Callvirt, Operand = new(nameof(Constructable), nameof(Constructable.SetState)) },
        { Pop, "Insert1" }
    };

    public static readonly List<CodeInstruction> InstructionsToAdd1 = new()
    {
        new(Ldloc_0),
        new(Call, Reflect.Method(() => GhostCreated(default)))
    };

    public static readonly InstructionsPattern AddInstructionPattern2 = new()
    {
        Ldloc_S,
        Ldloc_3,
        Ldloc_S,
        Or,
        { new() { OpCode = Callvirt, Operand = new(nameof(Constructable), nameof(Constructable.SetIsInside)) }, "Insert2" }
    };

    public static readonly List<CodeInstruction> InstructionsToAdd2 = new()
    {
        TARGET_METHOD.Ldloc<Constructable>(),
        new(Call, Reflect.Method(() => GhostCreated(default)))
    };


    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions) =>
        instructions.Transform(AddInstructionPattern1, (label, instruction) =>
        {
            if (label.Equals("Insert1"))
            {
                return InstructionsToAdd1;
            }
            return null;
        }).Transform(AddInstructionPattern2, (label, instruction) =>
        {
            if (label.Equals("Insert2"))
            {
                return InstructionsToAdd2;
            }
            return null;
        });
#elif BELOWZERO
    public static IEnumerable<CodeInstruction> Transpiler(MethodBase methodBase, IEnumerable<CodeInstruction> instructions, ILGenerator il)
    {
        Label newJump = il.DefineLabel();
        // First injection
        var Brfalse = new CodeInstruction(OpCodes.Brfalse_S, newJump);

        var dup = new CodeInstruction(OpCodes.Dup);
        dup.labels.Add(newJump);

        return new CodeMatcher(instructions).MatchEndForward([
            new CodeMatch(OpCodes.Ldloc_0),
            new CodeMatch(OpCodes.Ldc_I4_0),
            new CodeMatch(OpCodes.Ldc_I4_1),
            new CodeMatch(OpCodes.Callvirt,  Reflect.Method((Constructable constructable) => constructable.SetState(default, default))),
            new CodeMatch(Pop),
        ])
        .Advance(1)
        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_0))
        .Insert(new CodeInstruction(OpCodes.Call, Reflect.Method(() => GhostCreated(default))))
        // Second injection
        .MatchEndForward([
           new CodeMatch(OpCodes.Pop),
           new CodeMatch(OpCodes.Ldsfld),
           new CodeMatch(OpCodes.Ldnull),
           new CodeMatch(OpCodes.Call),
        ])
        .Advance(1)
        .RemoveInstruction()
        .Insert(Brfalse)
        // Third injection
        .MatchEndForward([
            new CodeMatch(OpCodes.Call, Reflect.Method(() => Object.Destroy(default))),
        ])
        .Advance(1)
        .Insert(dup)
        // Forth injection
        .MatchEndForward([
            new CodeMatch(OpCodes.Ldloc_S),
            new CodeMatch(OpCodes.Ldloc_S),
            new CodeMatch(OpCodes.Or),
            new CodeMatch(OpCodes.Callvirt,  Reflect.Method((Constructable constructable) => constructable.SetIsInside(default)))

        ])
        
        .Advance(1)
        .InsertAndAdvance(new CodeInstruction(OpCodes.Call, Reflect.Method(() => GhostCreated(default))))
        .InstructionEnumeration();
    }
#endif

    public static void GhostCreated(Constructable constructable)
    {
        GameObject ghostObject = constructable.gameObject;

        NitroxId parentId = null;
        if (ghostObject.TryGetComponentInParent(out SubRoot subRoot, true) && (subRoot.isBase || subRoot.isCyclops) &&
            subRoot.TryGetNitroxId(out NitroxId entityId))
        {
            parentId = entityId;
        }

        // Assign a NitroxId to the ghost now
        NitroxId ghostId = new();
        NitroxEntity.SetNewId(ghostObject, ghostId);
        if (constructable is ConstructableBase constructableBase)
        {
            GhostEntity ghost = GhostEntitySpawner.From(constructableBase);
            ghost.Id = ghostId;
            ghost.ParentId = parentId;
            Resolve<IPacketSender>().Send(new PlaceGhost(ghost));
        }
        else
        {
            ModuleEntitySpawner.MoveToGlobalRoot(ghostObject);
            
            ModuleEntity module = ModuleEntitySpawner.From(constructable);
            module.Id = ghostId;
            module.ParentId = parentId;
            Resolve<IPacketSender>().Send(new PlaceModule(module));

            if (constructable.transform.parent && constructable.transform.parent.TryGetComponent(out NitroxCyclops nitroxCyclops) && nitroxCyclops.Virtual)
            {
                nitroxCyclops.Virtual.ReplicateConstructable(constructable);
            }
        }
    }
}
